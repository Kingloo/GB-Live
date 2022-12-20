using System;

namespace GBLive.GiantBomb
{
	public static class Constants
	{
		public static string IsLiveMessage { get; } = "GiantBomb is LIVE";
		public static string IsNotLiveMessage { get; } = "GiantBomb is not streaming";
		public static string NameOfUntitledLiveShow { get; } = "Untitled Live Show";
		public static string NameOfNoLiveShow { get; } = "No live show";
		public static string UserAgent { get; } = "GB Live (GitHub) - Desktop app showing upcoming events and live status - polls /upcoming_json every 2 mins";
		
		public static Uri HomePage { get; } = new Uri("https://www.giantbomb.com/", UriKind.Absolute);
		public static Uri ChatPage { get; } = new Uri("https://www.giantbomb.com/chat/", UriKind.Absolute);
		public static Uri UpcomingJsonUri { get; } = new Uri("https://www.giantbomb.com/upcoming_json", UriKind.Absolute);
		public static Uri FallbackImage { get; } = new Uri("https://www.giantbomb.com/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png", UriKind.Absolute);

		public static TimeSpan UpdateInterval { get; } = TimeSpan.FromSeconds(90d);
	}
}
