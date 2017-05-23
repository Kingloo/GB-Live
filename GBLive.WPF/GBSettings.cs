using System;

namespace GBLive.WPF
{
    public class GBSettings
    {
        public string IsLiveMessage { get; set; } = "Giant Bomb is LIVE";
        public string IsNotLiveMessage { get; set; } = "Giant Bomb is not streaming";

        private static readonly Uri _gbHomePage = new Uri("https://www.giantbomb.com");
        public Uri GBHomePage => _gbHomePage;

        private static readonly Uri _gbChatPage = new Uri("https://www.giantbomb.com/chat");
        public Uri GBChatPage => _gbChatPage;

        private static readonly Uri _gbJson = new Uri("https://www.giantbomb.com/upcoming_json");
        public Uri GBJson => _gbJson;

        public string UserAgent => "GB Live (GitHub) - Desktop app for upcoming events and live status - polls upcoming_json every 2 mins";

        private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(2);
        public TimeSpan UpdateInterval => _updateInterval;

        public GBSettings() { }

        public GBSettings(TimeSpan updateInterval)
        {
            // we only assign to _updateInterval when the new one is a longer period of time
            // this ensures the interval minimum as the variable initialisation value

            if (updateInterval > _updateInterval)
            {
                _updateInterval = updateInterval;
            }
        }
    }
}
