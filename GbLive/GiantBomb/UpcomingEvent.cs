using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using GbLive.Common;
using Newtonsoft.Json.Linq;

namespace GbLive.GiantBomb
{
    public class UpcomingEvent : IComparable<UpcomingEvent>, IEquatable<UpcomingEvent>
    {
        private DispatcherCountdownTimer countdown = null;

        #region Properties
        private readonly string _title = string.Empty;
        public string Title => _title;

        private readonly DateTime _time = DateTime.MinValue;
        public DateTime Time => _time;

        private readonly bool _isPremium = false;
        public bool IsPremium => _isPremium;

        private readonly string _eventType = string.Empty;
        public string EventType => _eventType;

        private readonly Uri _image = Settings.FallbackImage;
        public Uri Image => _image;
        #endregion
        
        private UpcomingEvent(string title, DateTime time, bool isPremium, string eventType, Uri image)
        {
            _title = title;
            _time = time;
            _isPremium = isPremium;
            _eventType = eventType;
            _image = image;
        }

        public static bool TryCreate(JToken token, out UpcomingEvent outEvent)
        {
            if (token == null)
            {
                outEvent = null;
                return false;
            }

            if (!token.HasValues)
            {
                outEvent = null;
                return false;
            }

            string title = (string)token["title"] ?? "Untitled Event";
            DateTime time = GetTime(token);
            bool isPremium = GetPremium(token);
            string eventType = (string)token["type"] ?? "Unknown";
            Uri image = GetImage(token);

            outEvent = new UpcomingEvent(title, time, isPremium, eventType, image);

            return true;
        }

        private static DateTime GetTime(JToken token)
        {
            string time = (string)token["date"];

            if (DateTime.TryParse(time, out DateTime dt))
            {
                TimeSpan pacificUtcOffset = TimeZoneInfo
                    .GetSystemTimeZones()
                    .Single(i => i.Id == "Pacific Standard Time")
                    .GetUtcOffset(DateTime.UtcNow);

                TimeSpan localUtcOffset = TimeZoneInfo
                    .Local
                    .GetUtcOffset(DateTime.UtcNow);

                TimeSpan offset = localUtcOffset - pacificUtcOffset;

                return dt.Add(offset);
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "{0} could not be parsed into a DateTime", time);

                Log.LogMessage(errorMessage);

                return DateTime.MinValue;
            }
        }

        private static bool GetPremium(JToken token)
        {
            string premium = (string)token["premium"] ?? "false"; // false by default

            return Convert.ToBoolean(premium, CultureInfo.InvariantCulture);
        }

        private static Uri GetImage(JToken token)
        {
            string link = (string)token["image"];

            string decodedLink = WebUtility.HtmlDecode(link);

            if (!decodedLink.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                decodedLink = string.Format(CultureInfo.InvariantCulture, "https://{0}", decodedLink);
            }

            return Uri.TryCreate(decodedLink, UriKind.Absolute, out Uri uri) ? uri : Settings.FallbackImage;
        }

        public void StartCountdown()
        {
            if (countdown != null && countdown.IsCountdownRunning)
            {
                return;
            }

            Int64 ticksUntilTime = CalculateTicksUntilTime(Time);

            if (IsTicksValid(ticksUntilTime))
            {
                void goToHome() => Utils.OpenUriInBrowser(Settings.Home);
                void notify() => NotificationService.Send(Title, goToHome);

                countdown = new DispatcherCountdownTimer(TimeSpan.FromTicks(ticksUntilTime), notify);
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "{0}: countdown not started, {1} is too far in the future", Title, Time);

                Log.LogMessage(errorMessage);
            }
        }

        private static Int64 CalculateTicksUntilTime(DateTime time)
        {
            if (time < DateTime.Now)
            {
                return -1L;
            }

            TimeSpan fromNowToTime = time - DateTime.Now;
            
            TimeSpan untilShouldNotify = fromNowToTime.Add(TimeSpan.FromSeconds(1d));

            return untilShouldNotify.Ticks;
        }

        private static bool IsTicksValid(Int64 ticksUntilTime)
        {
            if (ticksUntilTime < 0L)
            {
                return false;
            }

            /*
                even though you can start a DispatcherTimer with a ticks of Int64,
                the equivalent number of milliseconds cannot exceed Int32.MaxValue
             
                https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
                -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntil = ticksUntilTime / 10_000;

            return millisecondsUntil <= Convert.ToInt64(Int32.MaxValue);
        }

        public void StopCountdown()
            => countdown?.Stop();

        public int CompareTo(UpcomingEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Time.CompareTo(other.Time);
        }

        public bool Equals(UpcomingEvent other)
        {
            if (other == null)
            {
                return false;
            }

            bool isSameTitle = Title == other.Title;
            bool isSameTime = Time == other.Time;
            bool areBothPremium = IsPremium == other.IsPremium;
            bool areSameEventType = EventType == other.EventType;
            bool isSameImage = Image == other.Image;

            return isSameTitle && isSameTime && areBothPremium && areSameEventType && isSameImage;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(Title);
            sb.AppendLine(Time.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Is Premium: {0}", IsPremium));
            sb.AppendLine(EventType);
            sb.AppendLine(Image.AbsoluteUri);
            sb.AppendLine(HowMuchTimeLeftOnCountdown(countdown));

            return sb.ToString();
        }

        private static string HowMuchTimeLeftOnCountdown(DispatcherCountdownTimer countdown)
            => countdown == null
                ? "Countdown not started or expired"
                : string.Format(CultureInfo.CurrentCulture, "Time left: {0}", countdown.TimeLeft);
    }
}
