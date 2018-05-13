﻿using System;
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
        public string Title { get; } = string.Empty;
        public DateTimeOffset Time { get; } = DateTimeOffset.MinValue;
        public bool IsPremium { get; } = false;
        public string EventType { get; } = string.Empty;
        public Uri Image { get; } = Settings.FallbackImage;
        #endregion
        
        private UpcomingEvent(string title, DateTimeOffset time, bool isPremium, string eventType, Uri image)
        {
            Title = title;
            Time = time;
            IsPremium = isPremium;
            EventType = eventType;
            Image = image;
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
            DateTimeOffset time = GetTime(token);
            bool isPremium = GetPremium(token);
            string eventType = (string)token["type"] ?? "Unknown";
            Uri image = GetImage(token);

            outEvent = new UpcomingEvent(title, time, isPremium, eventType, image);

            return true;
        }

        private static DateTimeOffset GetTime(JToken token)
        {
            string time = (string)token["date"];

            if (DateTimeOffset.TryParse(time, out DateTimeOffset dt))
            {
                TimeSpan pacificUtcOffset = TimeZoneInfo
                    .GetSystemTimeZones()
                    .Single(i => i.Id == "Pacific Standard Time")
                    .GetUtcOffset(DateTimeOffset.UtcNow);

                TimeSpan localUtcOffset = TimeZoneInfo
                    .Local
                    .GetUtcOffset(DateTimeOffset.UtcNow);

                TimeSpan offset = localUtcOffset - pacificUtcOffset;

                return dt.Add(offset);
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "{0} could not be parsed into a DateTime", time);

                Log.LogMessage(errorMessage);

                return DateTimeOffset.MinValue;
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
            if (countdown != null && countdown.IsRunning)
            {
                return;
            }

            Int64 ticksUntilTime = CalculateTicksUntilTime(Time);

            if (IsTicksValid(ticksUntilTime))
            {
                void goToHome() => Utils.OpenUriInBrowser(Settings.Home);
                void notify() => NotificationService.Send(Title, goToHome);

                countdown = new DispatcherCountdownTimer(TimeSpan.FromTicks(ticksUntilTime), notify);

                countdown.Start();
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "{0}: countdown not started, {1} is too far in the future", Title, Time);

                Log.LogMessage(errorMessage);
            }
        }

        private static Int64 CalculateTicksUntilTime(DateTimeOffset time)
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

                Int32.MaxValue milliseconds is about 24.58 days
             
                https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
                -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntil = ticksUntilTime / 10_000; // there are 10_000 ticks in 1 ms

            return millisecondsUntil <= Convert.ToInt64(Int32.MaxValue);
        }

        public void StopCountdown() => countdown?.Stop();

        public int CompareTo(UpcomingEvent other) => Time.CompareTo(other.Time);

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
            sb.AppendLine(IsPremium ? "Is Premium? true" : "Is Premium? false");
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
