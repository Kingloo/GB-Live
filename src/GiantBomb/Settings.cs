using System;
using System.Text;
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

        public Uri Home { get; set; }
        public Uri Chat { get; set; }
        public Uri Upcoming { get; set; }
        public Uri FallbackImage { get; set; }
        
        public double UpdateIntervalInSeconds { get; set; } = -1d;
        public bool ShouldNotify { get; set; } = false;

#nullable disable
        public Settings() { }
#nullable enable

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(base.ToString());

            sb.AppendLine($"{nameof(IsLiveMessage)}: {IsLiveMessage}");
            sb.AppendLine($"{nameof(IsNotLiveMessage)}: {IsNotLiveMessage}");
            sb.AppendLine($"{nameof(NameOfUntitledLiveShow)}: {NameOfUntitledLiveShow}");
            sb.AppendLine($"{nameof(NameOfNoLiveShow)}: {NameOfNoLiveShow}");
            sb.AppendLine($"{nameof(UserAgent)}: {UserAgent}");

            sb.AppendLine($"{nameof(Home)}: {(Home?.AbsoluteUri ?? "no home uri")}");
            sb.AppendLine($"{nameof(Chat)}: {(Chat?.AbsoluteUri ?? "no chat uri")}");
            sb.AppendLine($"{nameof(Upcoming)}: {(Upcoming?.AbsoluteUri ?? "no upcoming uri")}");
            sb.AppendLine($"{nameof(FallbackImage)}: {(FallbackImage?.AbsoluteUri ?? "no fallback image uri")}");

            sb.AppendLine($"{nameof(UpdateIntervalInSeconds)}: {UpdateIntervalInSeconds}");
            sb.AppendLine($"{nameof(ShouldNotify)}: {ShouldNotify}");

            return sb.ToString();
        }
    }
}