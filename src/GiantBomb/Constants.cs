namespace GBLive.GiantBomb
{
    public static class Constants
    {
        public static string IsLiveMessage { get; } = "GiantBomb is LIVE";
        public static string IsNotLiveMessage { get; } = "GiantBomb is not streaming";
        public static string NameOfUntitledLiveShow { get; } = "Untitled Live Show";
        public static string NameOfNoLiveShow { get; } = "No live show";
        public static string UserAgent { get; } = "GB Live (GitHub) - Desktop app showing upcoming events and live status - polls /upcoming_json every 2 mins";
    }
}
