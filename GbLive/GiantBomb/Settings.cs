using System;

namespace GbLive.GiantBomb
{
    public static class Settings
    {
        public static string IsLiveMessage { get; } = "Giant Bomb is LIVE";
        public static string IsNotLiveMessage { get; } = "Giant Bomb is not streaming";
        public static string NameOfUntitledLiveShow { get; } = "Untitled Live Show";
        public static string NameOfNoLiveShow { get; } = "No live show";
        public static string UserAgent { get; } = "GB Live (GitHub) - Desktop app showing upcoming events and live status - polls /upcoming_json every 2 mins";

        public static Uri Home { get; } = new Uri("https://www.giantbomb.com/", UriKind.Absolute);
        public static Uri Chat { get; } = new Uri("https://www.giantbomb.com/chat", UriKind.Absolute);
        public static Uri Upcoming { get; } = new Uri("https://www.giantbomb.com/upcoming_json", UriKind.Absolute);
        public static Uri FallbackImage { get; } = new Uri("https://static.giantbomb.com/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png", UriKind.Absolute);

        public static TimeSpan UpdateInterval { get; } = TimeSpan.FromMinutes(2d);
    }
}
