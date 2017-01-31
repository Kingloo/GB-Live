using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace GB_Live
{
    public class GBUpcomingEvent : ViewModelBase, IComparable<GBUpcomingEvent>, IEquatable<GBUpcomingEvent>
    {
        #region Fields
        private CountdownDispatcherTimer countdown = null;
        private readonly DateTime creationDate = DateTime.Now;

        private static Uri fallbackImageLink = new UriBuilder
        {
            Scheme = "http",
            Host = "static.giantbomb.com",
            Path = "/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png",
            Port = 80
        }
        .Uri;
        #endregion

        #region Properties
        public string Title { get; private set; }

        private DateTime _time = DateTime.MinValue;
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;

                RaisePropertyChanged(nameof(Time));
            }
        }

        public bool Premium { get; private set; }
        public string EventType { get; private set; }
        public Uri BackgroundImageLink { get; private set; }
        #endregion
        
        public GBUpcomingEvent(
            string title,
            DateTime time,
            bool premium,
            string eventType,
            Uri imageUri)
        {
            Title = title;
            Time = time;
            Premium = premium;
            EventType = eventType;
            BackgroundImageLink = imageUri;
        }
        
        public static bool TryCreate(JToken token, out GBUpcomingEvent outEvent)
        {
            if (token == null) { throw new ArgumentNullException(nameof(token)); }

            if (token.HasValues == false)
            {
                outEvent = null;

                return false;
            }
            
            string title = GetTitle(token);
            DateTime time = GetTime(token);
            bool isPremium = GetIsPremium(token);
            string type = GetEventType(token);
            Uri imageUri = GetBackgroundImageLink(token);

            outEvent = new GBUpcomingEvent(title, time, isPremium, type, imageUri);

            return true;
        }
        
        private static string GetTitle(JToken token)
        {
            return (string)token["title"] ?? "Unknown Event";
        }

        private static DateTime GetTime(JToken token)
        {
            string time = (string)token["date"];
            
            DateTime dt = DateTime.MinValue;

            if (DateTime.TryParse(time, out dt))
            {
                TimeSpan pacificUtcOffset = TimeZoneInfo
                    .GetSystemTimeZones()
                    .SingleOrDefault(i => i.Id.Equals("Pacific Standard Time"))
                    .GetUtcOffset(DateTime.Now);

                TimeSpan localUtcOffset = TimeZoneInfo
                    .Local
                    .GetUtcOffset(DateTime.Now);

                TimeSpan offset = localUtcOffset - pacificUtcOffset;

                dt = dt.Add(offset);
            }

            return dt;
        }
        
        private static bool GetIsPremium(JToken token)
        {
            string premium = (string)token["premium"];
            
            return String.IsNullOrWhiteSpace(premium) ? false : Convert.ToBoolean(premium, CultureInfo.InvariantCulture);
        }

        private static string GetEventType(JToken token)
        {
            return (string)token["type"] ?? "Unknown";
        }

        private static Uri GetBackgroundImageLink(JToken token)
        {
            string uri = (string)token["image"];

            string decoded = HttpUtility.HtmlDecode(uri);
            
            Uri tmp = null;

            return Uri.TryCreate(decoded, UriKind.Absolute, out tmp) ? tmp : fallbackImageLink;
        }
        
        public void StartCountdownTimer()
        {
            Int64 ticks = CalculateTicks();

            if (CanStartCountdownWithTicks(ticks))
            {
                countdown = new CountdownDispatcherTimer(new TimeSpan(ticks), countdown_Fire);
            }
        }

        private void countdown_Fire()
        {
            Time = DateTime.MaxValue;

            NotificationService.Send(Title, () =>
                {
                    Utils.OpenUriInBrowser(ConfigurationManager.AppSettings["GBHomepage"]);
                });
        }

        private Int64 CalculateTicks()
        {
            TimeSpan fromNowToEvent = Time - DateTime.Now;

            // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s => 10,000 * 1000 * 3 ticks in 3 secs == 30,000,000 ticks
            TimeSpan threeSecondsInTicks = new TimeSpan(10000 * 1000 * 3);

            TimeSpan addedExtraTime = fromNowToEvent.Add(threeSecondsInTicks);

            Int64 ticksUntilEvent = addedExtraTime.Ticks;

#if DEBUG
            return fromNowToEvent.Ticks;
#else
            return ticksUntilEvent;
#endif
        }

        private static bool CanStartCountdownWithTicks(Int64 ticks)
        {
            if (ticks <= 0L) { return false; }

            /*
            * even though you can start a DispatcherTimer with a ticks type of Int64,
            * the equivalent number of milliseconds cannot exceed Int32.MaxValue
            * 
            * https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
            * -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntilEvent = ticks / 10000; // there are 10,000 ticks in one millisecond
            
            return millisecondsUntilEvent <= Convert.ToInt64(Int32.MaxValue);
        }

        public void StopCountdownTimer()
        {
            countdown?.Stop();
        }
        
        public int CompareTo(GBUpcomingEvent other)
        {
            if (other == null) { return 1; }

            if (Time > other.Time)
            {
                return 1;
            }
            else if (Time < other.Time)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        
        public bool Equals(GBUpcomingEvent other)
        {
            if (other == null) { return false; }

            if (!Title.Equals(other.Title)) { return false; }

            if (!Time.Equals(other.Time)) { return false; }

            if (!Premium.Equals(other.Premium)) { return false; }

            if (!EventType.Equals(other.EventType)) { return false; }

            return true;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Title: {0}", Title));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Time: {0}", Time));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Created: {0}", creationDate.ToString(CultureInfo.CurrentCulture)));
            sb.AppendLine("Countdown:");
            sb.Append(countdown.ToString());
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Event type: {0}", EventType.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Premium: {0}", Premium.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Image url: {0}", BackgroundImageLink.AbsoluteUri));

            return sb.ToString();
        }
    }
}
