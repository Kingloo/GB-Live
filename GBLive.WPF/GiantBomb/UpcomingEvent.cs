using System;
using System.Linq;
using System.Text;
using GBLive.Extensions;
using GBLive.WPF.Common;
using Newtonsoft.Json.Linq;

namespace GBLive.WPF.GiantBomb
{
    public class UpcomingEvent : IComparable<UpcomingEvent>, IEquatable<UpcomingEvent>
    {
        private DispatcherCountdownTimer countdown = null;

        #region Properties
        public string Title { get; } = "Untitled Event";
        public DateTimeOffset Time { get; } = DateTimeOffset.MinValue;
        public bool IsPremium { get; } = false;
        public string EventType { get; } = "Unknown";
        public Uri Image { get; } = Settings.FallbackImage;
        #endregion

        public UpcomingEvent(string title, DateTimeOffset date, bool isPremium, string eventType, Uri image)
        {
            Title = title;
            Time = date;
            IsPremium = isPremium;
            EventType = eventType;
            Image = image;
        }

        public static bool TryCreate(JObject json, out UpcomingEvent ue)
        {
            if (json == null)
            {
                ue = null;
                return false;
            }

            if (!json.HasValues)
            {
                ue = null;
                return false;
            }

            var sc = StringComparison.Ordinal;

            if (!json.TryGetValue("title", sc, out JToken titleToken)
                || !json.TryGetValue("date", sc, out JToken dateToken)
                || !json.TryGetValue("premium", sc, out JToken isPremiumToken)
                || !json.TryGetValue("type", sc, out JToken eventTypeToken)
                || !json.TryGetValue("image", sc, out JToken imageToken))
            {
                ue = null;
                return false;
            }

            string title = titleToken.Value<string>();

            DateTimeOffset date = GetDate(dateToken.Value<string>());

            bool isPremium = isPremiumToken.Value<bool>();

            string eventType = eventTypeToken.Value<string>();

            Uri image = GetImageUri(imageToken.Value<string>());


            ue = new UpcomingEvent(title, date, isPremium, eventType, image);
            return true;
        }

        private static DateTimeOffset GetDate(string raw)
        {
            if (!DateTimeOffset.TryParse(raw, out DateTimeOffset dt))
            {
                return DateTimeOffset.MaxValue;
            }

            TimeSpan pacificUtcOffset = TimeZoneInfo
                .GetSystemTimeZones()
                .Single(tz => tz.Id == "Pacific Standard Time")
                .GetUtcOffset(DateTimeOffset.UtcNow);

            TimeSpan localUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.UtcNow);

            TimeSpan offset = localUtcOffset - pacificUtcOffset;

            return dt.Add(offset);
        }

        private static Uri GetImageUri(string raw)
        {
            try
            {
                return new UriBuilder(raw)
                {
                    Port = 443,
                    Scheme = "https"
                }
                .Uri;
            }
            catch (UriFormatException)
            {
                return Settings.FallbackImage;
            }
        }

        public void StartCountdown()
        {
            if (countdown != null && countdown.IsRunning)
            {
                return;
            }

            Int64 ticksUntilEvent = CalculateTicksUntilEvent(Time);

            if (IsTicksValid(ticksUntilEvent))
            {
                void goToHome() => Settings.Home.OpenInBrowser();
                void notify() => NotificationService.Send(Title, goToHome);

                countdown = new DispatcherCountdownTimer(TimeSpan.FromTicks(ticksUntilEvent), notify);
                
                countdown.Start();
            }
        }

        public void StopCountdown()
        {
            if (countdown != null)
            {
                countdown.Stop();
            }
        }

        private Int64 CalculateTicksUntilEvent(DateTimeOffset date)
        {
            if (date < DateTimeOffset.Now) { return -1L; }

            TimeSpan fromNowUntilEvent = Time - DateTimeOffset.Now;

            TimeSpan untilShouldNotify = fromNowUntilEvent.Add(TimeSpan.FromSeconds(2d));

            return untilShouldNotify.Ticks;
        }

        private static bool IsTicksValid(Int64 ticksUntilEvent)
        {
            if (ticksUntilEvent < 0L) { return false; }

            /*
                even though you can start a DispatcherTimer with a ticks of Int64,
                the equivalent number of milliseconds cannot exceed Int32.MaxValue
                Int32.MaxValue milliseconds is about 24.58 days
             
                https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
                -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntilEvent = ticksUntilEvent / 10_000;

            return millisecondsUntilEvent <= Convert.ToInt64(Int32.MaxValue);
        }

        private void Countdown_Tick(object sender, EventArgs e)
        {
            StopCountdown();

            void goToHome() => Settings.Home.OpenInBrowser();

            NotificationService.Send(Title, goToHome);
        }

        public int CompareTo(UpcomingEvent other) => other == null ? 1 : Time.CompareTo(other.Time);

        public bool Equals(UpcomingEvent other)
        {
            if (other == null) { return false; }

            var sc = StringComparison.Ordinal;

            bool sameTitle = Title.Equals(other.Title, sc);
            bool sameDate = Time == other.Time;
            bool isAlsoPremium = IsPremium == other.IsPremium;
            bool sameEventType = EventType.Equals(other.EventType, sc);
            bool sameImage = Image == other.Image;

            return sameTitle && sameDate && isAlsoPremium && sameEventType && sameImage;
        }

        public override bool Equals(object obj) => (obj is UpcomingEvent ue) ? Equals(ue) : false;

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine($"title: {Title}");
            sb.AppendLine(IsPremium ? "is premium: true" : "is premium: false");
            sb.AppendLine($"event type: {EventType}");
            sb.AppendLine($"image uri: {Image.AbsoluteUri}");

            if (countdown != null && countdown.IsRunning)
            {
                sb.AppendLine($"time left: {countdown.TimeLeft.ToString()}");
            }

            return sb.ToString();
        }
    }
}
