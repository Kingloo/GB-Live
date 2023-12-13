using System;

namespace GBLive.Options
{
	public class GBLiveOptions
	{
		public string IsLiveMessage { get; init; } = string.Empty;
		public string IsNotLiveMessage { get; init; } = string.Empty;
		public string NameOfNoLiveShow { get; init; } = string.Empty;
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public string UserAgent { get; init; } = string.Empty;

		public GBLiveOptions() { }
	}
}
