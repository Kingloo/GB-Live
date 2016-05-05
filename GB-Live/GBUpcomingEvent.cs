using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace GB_Live
{
    public enum GBEventType { Unknown, Article, Review, Podcast, Video, LiveShow };

    public class GBUpcomingEvent : ViewModelBase, IComparable<GBUpcomingEvent>, IEquatable<GBUpcomingEvent>
    {
        #region Fields
        private CountdownDispatcherTimer countdown = null;
        private readonly DateTime creationDate = DateTime.Now;
        private static Uri fallbackImage = new UriBuilder
        {
            Scheme = "http",
            Host = "static.giantbomb.com",
            Path = "/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png",
            Port = 80
        }.Uri;
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

                OnNotifyPropertyChanged();
            }
        }
        public bool Premium { get; private set; }
        public GBEventType EventType { get; private set; }
        public Uri BackgroundImageUrl { get; private set; }
        #endregion
        
        private GBUpcomingEvent(string title, DateTime time, bool premium, GBEventType type, Uri imageUri)
        {
            Title = title;
            Time = time;
            Premium = premium;
            EventType = type;
            BackgroundImageUrl = imageUri;
        }
        
        public static bool TryCreate(JObject token, out GBUpcomingEvent outEvent)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            if (token.HasValues == false)
            {
                outEvent = null;

                return false;
            }
            
            string title = GetTitle(token);
            DateTime time = GetTime(token);
            bool isPremium = GetIsPremium(token);
            GBEventType type = GetEventType(token);
            Uri imageUri = GetBackgroundImageUrl(token);

            outEvent = new GBUpcomingEvent(title, time, isPremium, type, imageUri);

            return true;
        }
        
        private static string GetTitle(JObject token)
        {
            string title = (string)token["title"];
            
            return String.IsNullOrWhiteSpace(title) ? "Title Unknown" : title;
        }

        private static DateTime GetTime(JObject token)
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

                return dt.Add(offset);
            }
            else
            {
                return DateTime.MinValue;
            }
        }
        
        private static bool GetIsPremium(JObject token)
        {
            string premium = (string)token["premium"];

            return String.IsNullOrWhiteSpace(premium) ? false : Convert.ToBoolean(premium);
        }

        private static GBEventType GetEventType(JToken token)
        {
            GBEventType toReturn = GBEventType.Unknown;
            string type = (string)token["type"];

            switch (type)
            {
                case "Video":
                    toReturn = GBEventType.Video;
                    break;
                case "Live Show":
                    toReturn = GBEventType.LiveShow;
                    break;
                case "Article":
                    toReturn = GBEventType.Article;
                    break;
                case "Podcast":
                    toReturn = GBEventType.Podcast;
                    break;
                case "Review":
                    toReturn = GBEventType.Review;
                    break;
                default:
                    toReturn = GBEventType.Unknown;
                    break;
            }

            return toReturn;
        }

        private static Uri GetBackgroundImageUrl(JObject token)
        {
            string uri = (string)token["image"];

            string decoded = HttpUtility.HtmlDecode(uri);
            
            Uri tmp = null;

            return Uri.TryCreate(decoded, UriKind.Absolute, out tmp) ? tmp : fallbackImage;
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

            // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s => 10,000 * 1000 * 20 ticks in 20 secs == 200,000,000 ticks
            TimeSpan twentySecondsInTicks = new TimeSpan(10000 * 1000 * 20);

            TimeSpan addedExtraTime = fromNowToEvent.Add(twentySecondsInTicks);

            Int64 ticksUntilEvent = addedExtraTime.Ticks;

#if DEBUG
            return fromNowToEvent.Ticks;
#else
            return ticksUntilEvent;
#endif
        }

        private static bool CanStartCountdownWithTicks(Int64 ticks)
        {
            if (ticks <= 0L) return false;

            /*
            * even though you can start a DispatcherTimer with a ticks type of Int64,
            * the equivalent number of milliseconds cannot exceed Int32.MaxValue
            * 
            * http://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
            * -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntilEvent = ticks / 10000;

            if (millisecondsUntilEvent > Convert.ToInt64(Int32.MaxValue))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void StopCountdownTimer()
        {
            countdown?.Stop();
        }
        
        public int CompareTo(GBUpcomingEvent other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

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
            if (other == null) return false;

            if (Title.Equals(other.Title) == false) return false;

            if (Time.Equals(other.Time) == false) return false;

            if (Premium.Equals(other.Premium) == false) return false;

            if (EventType.Equals(other.EventType) == false) return false;

            return true;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Title: {0}", Title));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Time: {0}", Time));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Created: {0}", creationDate.ToString()));
            sb.AppendLine("Countdown:");
            sb.Append(countdown.ToString());
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Event type: {0}", EventType.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Premium: {0}", Premium.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Image url: {0}", BackgroundImageUrl.AbsoluteUri));

            return sb.ToString();
        }
    }
}
