using System;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;
using NUnit.Framework;

namespace GBLive.Tests.GiantBomb
{
	[TestFixture]
	public class ShowTests
	{
		[TestCase("fred", "fred")]
		[TestCase("FRED", "FRED")]
		public void Equals_TrueWhenTitlesAreSame(string title1, string title2)
		{
			IShow show1 = new Show { Title = title1 };
			IShow show2 = new Show { Title = title2 };

			Assert.IsTrue(show1.Equals(show2));
		}

		[TestCase("fred", "james")]
		[TestCase("FRED", "fred")]
		public void Equals_FalseWhenTitlesAreDifferent(string title1, string title2)
		{
			IShow show1 = new Show { Title = title1 };
			IShow show2 = new Show { Title = title2 };

			Assert.IsFalse(show1.Equals(show2));
		}

		[Test]
		public void CompareTo()
		{
			IShow show1 = new Show { Time = DateTimeOffset.Now.AddDays(1d) };
			IShow show2 = new Show { Time = DateTimeOffset.Now.AddDays(2d) };

			bool earlier = show1.CompareTo(show2) < 0;

			Assert.IsTrue(earlier);
		}

		[TestCase(10000000000L)]
		[TestCase(Int32.MaxValue - 1)]
		public void TimerDoesStartWhenTimeIsValid(Int64 ticks)
		{
			IShow show = new Show
			{
				Time = DateTimeOffset.Now.Add(TimeSpan.FromTicks(ticks))
			};

			show.StartCountdown(() => { });

			Assert.IsTrue(show.IsCountdownTimerRunning);
		}

		[Test]
		public void CountdownTimer_ShouldNotStartWhenShowTimeIsTooFarInTheFuture()
		{
			IShow show = new Show
			{
				Time = DateTimeOffset.Now.Add(TimeSpan.FromDays(25d))
			};

			show.StartCountdown(() => { });

			Assert.IsFalse(show.IsCountdownTimerRunning, "countdown shouldn't have started, but it did");
		}

		[Test]
		public void CountdownTimer_ShouldNotStartWhenTimeIsEarlierThanNow()
		{
			IShow show = new Show
			{
				Time = DateTimeOffset.Now.Add(TimeSpan.FromDays(-1d))
			};

			show.StartCountdown(() => { });

			Assert.IsFalse(show.IsCountdownTimerRunning);
		}

		[Test]
		public void StopCountdown_ShouldStopCountdown()
		{
			IShow show = new Show
			{
				Time = DateTimeOffset.Now.AddDays(1d)
			};

			show.StartCountdown(() => { });

			show.StopCountdown();

			Assert.IsFalse(show.IsCountdownTimerRunning, "the countdown shouldn't have been running, but it was");
		}
	}
}
