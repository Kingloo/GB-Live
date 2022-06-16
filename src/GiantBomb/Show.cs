using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using GBLive.Common;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.GiantBomb
{
	public class Show : IShow
	{
		private DispatcherCountdownTimer? countdown;

		public bool IsCountdownTimerRunning => countdown?.IsRunning ?? false;

		public string Title { get; set; } = "Untitled";
		public DateTimeOffset Time { get; set; } = DateTimeOffset.MinValue;
		public bool IsPremium { get; set; } = false;
		public string ShowType { get; set; } = "Unknown";
		public Uri Image { get; set; } = new Uri("https://www.giantbomb.com/");

		public Show() { }

		public void StartCountdown(Action notify)
		{
			if (countdown != null && countdown.IsRunning) { return; }

			Int64 ticksUntilShow = CalculateTicks(Time);

			if (!IsTicksValid(ticksUntilShow)) { return; }

			TimeSpan timeUntilShow = TimeSpan.FromTicks(ticksUntilShow);

			countdown = new DispatcherCountdownTimer(timeUntilShow, notify);

			countdown.Start();
		}

		private static Int64 CalculateTicks(DateTimeOffset dto)
		{
			if (dto < DateTimeOffset.Now) { return -1L; }

			TimeSpan fromNowUntilShow = dto - DateTimeOffset.Now;

			TimeSpan untilShouldNotify = fromNowUntilShow.Add(TimeSpan.FromSeconds(10d));

			return untilShouldNotify.Ticks;
		}

		private static bool IsTicksValid(Int64 ticks)
		{
			/*
                even though you can start a DispatcherTimer with a ticks of Int64,
                the equivalent number of milliseconds cannot exceed Int32.MaxValue
                Int32.MaxValue's worth of milliseconds is ~ 24.58 days

                https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs

                or

                https://github.com/dotnet/wpf/blob/master/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherTimer.cs

                -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

			if (ticks < 0L) { return false; }

			Int64 msUntilShow = ticks / 10_000;

			return msUntilShow <= Convert.ToInt64(Int32.MaxValue);
		}

		public void StopCountdown()
		{
			if (countdown != null)
			{
				countdown.Stop();

				countdown = null;
			}
		}

		public int CompareTo([AllowNull] IShow other) => (other is Show show) ? Time.CompareTo(show.Time) : 1; // we treat null-other as always earlier

		public bool Equals([AllowNull] IShow other) => (other is Show show) && EqualsInternal(show);

		private bool EqualsInternal(Show show)
		{
			var sco = StringComparison.Ordinal;

			bool sameTitle = Title.Equals(show.Title, sco);
			bool sameTime = Time.EqualsExact(show.Time);
			bool samePremium = IsPremium == show.IsPremium;
			bool sameShowType = ShowType.Equals(show.ShowType, sco);
			bool sameImage = AreLinksEqual(Image, show.Image);

			return sameTitle && sameTime && samePremium && sameShowType && sameImage;
		}

		private static bool AreLinksEqual(Uri? one, Uri? two)
		{
			if (one is null && two is null)
			{
				return true;
			}

			if ((one is null) != (two is null))
			{
				return false;
			}

			if ((!(one is null) && (!(two is null))))
			{
				return one.Equals(two);
			}

			return false;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(base.ToString());
			sb.AppendLine(IsCountdownTimerRunning ? "countdown running" : "countdown NOT running");
			sb.AppendLine(Title);
			sb.AppendLine(Time.ToString(CultureInfo.CurrentCulture));
			sb.AppendLine(IsPremium ? "premium" : "not premium");
			sb.AppendLine(ShowType);
			sb.AppendLine(Image?.AbsoluteUri ?? "no image uri");

			return sb.ToString();
		}
	}
}
