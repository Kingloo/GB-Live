using System;
using System.Globalization;
using System.Text;
using System.Windows.Threading;

namespace GB_Live
{
    public class CountdownDispatcherTimer
    {
        #region Fields
        private readonly DateTime created = DateTime.Now;
        private readonly Action tick = null;
        private DispatcherTimer timer = null;
        #endregion

        #region Properties
        public bool IsActive
        {
            get
            {
                return (timer != null);
            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                return IsActive ? ((created + timer.Interval) - DateTime.Now) : TimeSpan.Zero;
            }
        }
        #endregion

        public CountdownDispatcherTimer(DateTime time, Action tick)
        {
            if (time < DateTime.Now) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "{0} is in the past, it must be in the future", time.ToString()));
            if (tick == null) throw new ArgumentNullException(nameof(tick));

            this.tick = tick;

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan((time - DateTime.Now).Ticks)
            };

            timer.Tick += Timer_Tick;
            timer.Start();
        }
        
        public CountdownDispatcherTimer(TimeSpan span, Action tick)
        {
            // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s == 10,000,000 ticks
            if (span.Ticks < (10000 * 1000)) throw new ArgumentException("span.Ticks cannot be less than 1 second", nameof(span));
            if (tick == null) throw new ArgumentNullException(nameof(tick));

            this.tick = tick;

            timer = new DispatcherTimer
            {
                Interval = span
            };

            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tick();

            Stop();
        }

        public void Stop()
        {
            if (IsActive)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;

                timer = null;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().ToString());
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Created at: {0}", created.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Active: {0}", IsActive));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Time left: {0}", TimeLeft.ToString()));

            return sb.ToString();
        }
    }
}