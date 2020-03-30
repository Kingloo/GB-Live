using System;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.GiantBomb
{
    public class Settings : ISettings
    {
        public string IsLiveMessage { get; set; } = string.Empty;
        public string IsNotLiveMessage { get; set; } = string.Empty;
        public string NameOfUntitledLiveShow { get; set; } = string.Empty;
        public string NameOfNoLiveShow { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        public Uri? Home { get; set; }
        public Uri? Chat { get; set; }
        public Uri? Upcoming { get; set; }
        public Uri? FallbackImage { get; set; }
        
        public double UpdateIntervalInSeconds { get; set; } = -1d;
        public bool ShouldNotify { get; set; } = false;
        
        public Settings() { }
    }
}