using System;

namespace GBLive.GiantBomb.Interfaces
{
    public interface ISettings
    {
        string IsLiveMessage { get; set; }
        string IsNotLiveMessage { get; set; }
        string NameOfUntitledLiveShow { get; set; }
        string NameOfNoLiveShow { get; set; }
        string UserAgent { get; set; }

        Uri? Home { get; set; }
        Uri? Chat { get; set; }
        Uri? Upcoming { get; set; }
        Uri? FallbackImage { get; set; }

        int UpdateIntervalInSeconds { get; set; }
        bool ShouldNotify { get; set; }
    }
}
