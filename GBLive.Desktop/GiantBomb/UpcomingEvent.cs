using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using GBLive.Desktop.Common;
using Newtonsoft.Json.Linq;

namespace GBLive.Desktop.GiantBomb
{
    public class UpcomingEvent : IComparable<UpcomingEvent>, IEquatable<UpcomingEvent>
    {
        #region Fields
        private DispatcherCountdownTimer countdownTimer = default(DispatcherCountdownTimer);

        private static Uri fallbackImageUri
            = new Uri("https://static.giantbomb.com/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png");

        private readonly Action _countdownAction = null;
        #endregion

        #region Properties
        private readonly string _title = string.Empty;
        public string Title => _title;

        private readonly DateTime _time = DateTime.MinValue;
        public DateTime Time => _time;

        private readonly bool _isPremium = false;
        public bool IsPremium => _isPremium;

        private readonly string _eventType = string.Empty;
        public string EventType => _eventType;

        private readonly Uri _imageUri = default(Uri);
        public Uri ImageUri => _imageUri;
        #endregion
        
        private UpcomingEvent(
            string title,
            DateTime time,
            bool isPremium,
            string eventType,
            Uri imageUri,
            Action action)
        {
            _title = title;
            _time = time;
            _isPremium = isPremium;
            _eventType = eventType;
            _imageUri = imageUri;
            _countdownAction = action;
        }

        public static bool TryCreate(JToken token, Action action, out UpcomingEvent outEvent)
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
            
            string title = GetTitle(token);
            DateTime time = GetTime(token);
            bool isPremium = GetPremium(token);
            string eventType = GetEventType(token);
            Uri imageUri = GetImageUri(token);
            
            outEvent = new UpcomingEvent(title, time, isPremium, eventType, imageUri, action);

            return true;
        }
        
        private static string GetTitle(JToken token)
        {
            return (string)token["title"] ?? "Untitled Event";
        }

        private static DateTime GetTime(JToken token)
        {
            string time = (string)token["date"];

            DateTime dt = DateTime.MinValue;

            if (DateTime.TryParse(time, out dt))
            {
                TimeSpan pacificUtcOffset = TimeZoneInfo
                    .GetSystemTimeZones()
                    .Single(i => i.Id.Equals("Pacific Standard Time"))
                    .GetUtcOffset(DateTime.Now);

                TimeSpan localUtcOffset = TimeZoneInfo
                    .Local
                    .GetUtcOffset(DateTime.Now);

                TimeSpan offset = localUtcOffset - pacificUtcOffset;

                dt = dt.Add(offset);
            }

            return dt;
        }

        private static bool GetPremium(JToken token)
        {
            string premium = (string)token["premium"];

            return String.IsNullOrWhiteSpace(premium) ? false : Convert.ToBoolean(premium, CultureInfo.InvariantCulture);
        }

        private static string GetEventType(JToken token)
        {
            return (string)token["type"] ?? "Unknown";
        }

        private static Uri GetImageUri(JToken token)
        {
            string uriAsString = (string)token["image"];

            string decoded = WebUtility.HtmlDecode(uriAsString);

            if (!decoded.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                decoded = string.Format(CultureInfo.InvariantCulture, "https://{0}", decoded);
            }

            return Uri.TryCreate(decoded, UriKind.Absolute, out Uri uri) ? uri : fallbackImageUri;
        }

        public void StartCountdownTimer()
        {
            Int64 ticks = CalculateTicksUntilTime(Time);

            if (IsValidNumberOfTicks(ticks))
            {
                countdownTimer = new DispatcherCountdownTimer(TimeSpan.FromTicks(ticks), CountdownTimer_Fire);

                countdownTimer.Start();
            }
        }

        private static bool IsValidNumberOfTicks(Int64 ticks)
        {
            if (ticks < 0L) { return false; }

            /*
            even though you can start a DispatcherTimer with a ticks type of Int64,
            the equivalent number of milliseconds cannot exceed Int32.MaxValue
             
            https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
            -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            // there are 10,000 ticks in one millisecond
            Int64 millisecondsUntilNotify = ticks / 10_000;

            return millisecondsUntilNotify <= Convert.ToInt64(Int32.MaxValue);
        }

        private static Int64 CalculateTicksUntilTime(DateTime time)
        {
            TimeSpan fromNowtoTime = time - DateTime.Now;

            TimeSpan threeSecondsInTicks = new TimeSpan(10_000 * 1000 * 3);

            Int64 ticksUntilShouldNotify = (fromNowtoTime.Add(threeSecondsInTicks)).Ticks;

            return ticksUntilShouldNotify;
        }

        private void CountdownTimer_Fire()
        {
            _countdownAction?.Invoke();
            
            // this shouldn't be necessary, but it does no harm
            countdownTimer?.Stop();
        }

        public void StopCountdownTimer()
        {
            countdownTimer?.Stop();
        }

        public int CompareTo(UpcomingEvent other)
        {
            if (other == null) { throw new ArgumentNullException(nameof(other)); }

            return Time.CompareTo(other.Time);
        }

        public bool Equals(UpcomingEvent other)
        {
            if (other == null) { return false; }
            
            bool sameTitle = Title.Equals(other.Title);
            bool sameTime = Time.Equals(other.Time);
            bool samePremium = IsPremium.Equals(other.IsPremium);
            bool sameEventType = EventType.Equals(other.EventType);
            bool sameImageUri = ImageUri.Equals(other.ImageUri);

            return sameTitle && sameTime && samePremium && sameEventType && sameImageUri;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine(GetType().FullName);
            sb.AppendLine(Title);
            sb.AppendLine(Time.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(IsPremium.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(EventType);
            sb.AppendLine(ImageUri.AbsoluteUri);
            sb.AppendLine(CountdownTimeLeft(countdownTimer));

            return sb.ToString();
        }

        private static string CountdownTimeLeft(DispatcherCountdownTimer timer)
        {
            if (timer is null)
            {
                return "Timer not started or expired";
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, "Countdown time to event: {0}", timer.TimeLeft);
            }
        }
    }
}
