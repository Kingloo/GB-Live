using System;
using System.Globalization;
using System.Text;
using System.Windows.Threading;

namespace GBLive.Common
{
	internal sealed class DispatcherCountdownTimer
	{
		private readonly TimeSpan span;
		private readonly Action tick;

		private readonly DateTime created = DateTime.Now;
		private DispatcherTimer? timer = null;

		public bool IsRunning => timer?.IsEnabled ?? false;

		public TimeSpan TimeLeft => IsRunning
			? (created + (timer?.Interval ?? TimeSpan.Zero) - DateTime.Now)
			: TimeSpan.Zero;

		public DispatcherCountdownTimer(TimeSpan span, Action tick)
		{
			// there are 10_000 ticks in 1 millisecond
			// therefore there are 10_000 * 1000 ticks in 1 second
			// 10_000 * 1000 = 10_000_000

			ArgumentOutOfRangeException.ThrowIfLessThan<Int64>(span.Ticks, 10_000 * 1000);
			ArgumentNullException.ThrowIfNull(tick);

			this.tick = tick;
			this.span = span;
		}

		public void Start()
		{
			timer = new DispatcherTimer(DispatcherPriority.Background)
			{
				Interval = span
			};

			timer.Tick += Timer_Tick;

			timer.Start();
		}

		private void Timer_Tick(object? sender, EventArgs e)
		{
			tick();

			Stop();
		}

		public void Stop()
		{
			if (timer is not null)
			{
				timer.Stop();
				timer.Tick -= Timer_Tick;
				timer = null;
			}
		}

		public override string ToString()
		{
			CultureInfo cc = CultureInfo.CurrentCulture;

			return new StringBuilder()
				.AppendLine(GetType().FullName)
				.AppendLine(GetType().FullName)
				.AppendLine(string.Format(cc, "Created at: {0}", created.ToString(cc)))
				.AppendLine(IsRunning ? "Is Running: true" : "Is Running: false")
				.AppendLine(string.Format(cc, "Time left: {0}", TimeLeft.ToString()))
				.ToString();
		}
	}
}
