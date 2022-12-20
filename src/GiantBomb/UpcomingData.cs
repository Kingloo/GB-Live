using System;
using System.Collections.Generic;

namespace GBLive.GiantBomb
{
	public class UpcomingData
	{
		public LiveNowData? LiveNow { get; init; }
		public IList<Show> Upcoming { get; init; } = Array.Empty<Show>();
	}
}
