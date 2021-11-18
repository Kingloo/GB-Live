using System;

namespace GBLive.GiantBomb.Interfaces
{
	public interface IShow : IEquatable<IShow>, IComparable<IShow>
	{
		bool IsCountdownTimerRunning { get; }

		string Title { get; set; }
		DateTimeOffset Time { get; set; }
		bool IsPremium { get; set; }
		string ShowType { get; set; }
		Uri Image { get; set; }

		void StartCountdown(Action notify);
		void StopCountdown();
	}
}
