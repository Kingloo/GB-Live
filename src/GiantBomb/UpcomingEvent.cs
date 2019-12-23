using System;
using System.Globalization;
using System.Text;
using GBLive.Common;
using GBLive.Extensions;

namespace GBLive.GiantBomb
{
    public class UpcomingEvent : IEquatable<UpcomingEvent>
    {
        private DispatcherCountdownTimer? countdown = null;

        public string Title { get; set; } = "Untitled Event";
        public DateTimeOffset Time { get; set; } = DateTimeOffset.MinValue;
        public bool IsPremium { get; set; } = false;
        public string EventType { get; set; } = "Unknown";
        public Uri Image { get; set; } = Settings.FallbackImage;

        public UpcomingEvent() { }

        public void StartCountdown()
        {
            if (countdown != null && countdown.IsRunning)
            {
                return;
            }

            Int64 ticksUntilEvent = CalculateTicks(Time);

            if (!IsTicksValid(ticksUntilEvent))
            {
                return;
            }

            TimeSpan countdownTime = TimeSpan.FromTicks(ticksUntilEvent);

            void goToHome() => Settings.Home.OpenInBrowser();
            void notify() => NotificationService.Send(Title, goToHome);

            countdown = new DispatcherCountdownTimer(countdownTime, notify);
            
            countdown.Start();
        }

        private static Int64 CalculateTicks(DateTimeOffset date)
        {
            if (date < DateTimeOffset.Now) { return -1L; }

            TimeSpan fromNowUntilEvent = date - DateTimeOffset.Now;

            TimeSpan untilShouldNotify = fromNowUntilEvent.Add(TimeSpan.FromSeconds(2d));

            return untilShouldNotify.Ticks;
        }

        private static bool IsTicksValid(Int64 ticks)
        {
            if (ticks < 0L)
            {
                return false;
            }

            /*
                even though you can start a DispatcherTimer with a ticks of Int64,
                the equivalent number of milliseconds cannot exceed Int32.MaxValue
                Int32.MaxValue's worth of milliseconds is about 24.58 days
             
                https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs

                or

                https://github.com/dotnet/wpf/blob/master/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherTimer.cs

                -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntilEvent = ticks / 10_000;

            return millisecondsUntilEvent <= Convert.ToInt64(Int32.MaxValue);
        }

        public void StopCountdown() => countdown?.Stop();

        public bool Equals(UpcomingEvent other)
        {
            if (other is null)
            {
                return false;
            }

            bool sameTitle = Title.Equals(other.Title, StringComparison.Ordinal);
            bool sameTime = Time.EqualsExact(other.Time);
            bool samePremium = IsPremium == other.IsPremium;
            bool sameEventType = EventType.Equals(other.EventType, StringComparison.Ordinal);
            bool sameImage = Image.Equals(other.Image);

            return sameTitle && sameTime && samePremium && sameEventType && sameImage;
        }

        public override bool Equals(object? obj) => (obj is UpcomingEvent ue) ? Equals(ue) : false;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Title);
            sb.AppendLine(Time.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(IsPremium.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(EventType);
            sb.AppendLine(Image.AbsoluteUri);

            return sb.ToString();
        }
    }
}