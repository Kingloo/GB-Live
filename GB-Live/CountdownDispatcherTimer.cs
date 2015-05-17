using System;
using System.Windows.Threading;

namespace GB_Live
{
    public class CountdownDispatcherTimer
    {
        private Action tick = null;

        public CountdownDispatcherTimer(DateTime time, Action tick)
        {
            if (time < DateTime.Now) throw new ArgumentException(string.Format("{0} is in the past, it must be in the future", time.ToString()));
            if (tick == null) throw new ArgumentNullException("tick event is null");

            this.tick = tick;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = new TimeSpan((time - DateTime.Now).Ticks)
            };

            timer.Tick += timer_Tick;
            timer.Start();
        }

        public CountdownDispatcherTimer(TimeSpan span, Action tick)
        {
            if (span.Ticks < 10000000L) throw new ArgumentException("span cannot be less than 1 second"); // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s == 10,000,000 ticks
            if (tick == null) throw new ArgumentNullException("tick event is null");

            this.tick = tick;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = span
            };

            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            tick();

            DispatcherTimer timer = (DispatcherTimer)sender;

            timer.Stop();
            timer.Tick -= timer_Tick;

            timer = null;
        }
    }
}