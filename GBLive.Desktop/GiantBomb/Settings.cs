using System;

namespace GBLive.Desktop.GiantBomb
{
    public static class Settings
    {
        public static string IsLiveMessage { get; } = "Giant Bomb is LIVE";
        public static string IsNotLiveMessage { get; } = "Giant Bomb is not streaming";

        public static Uri Homepage { get; } = new Uri("https://www.giantbomb.com");
        public static Uri ChatPage { get; } = new Uri("https://www.giantbomb.com/chat");
        public static Uri UpcomingJson { get; } = new Uri("https://www.giantbomb.com/upcoming_json");
        public static string UserAgent { get; } = "GB Live (GitHub) - Desktop app showing upcoming events and live status - polls /upcoming_json every 2 mins";

        public static TimeSpan UpdateInterval { get; } = TimeSpan.FromMinutes(2);
    }
}
