using System.Collections.Generic;
using System.Text;

namespace GbLive.GiantBomb
{
    public class UpcomingResponse
    {
        public bool WasSuccessful { get; } = true;
        public string ErrorMessage { get; } = string.Empty;
        public string LiveShowName { get; } = Settings.NameOfNoLiveShow;
        public bool IsLive { get; } = false;
        public IReadOnlyList<UpcomingEvent> Events { get; } = null;

        public UpcomingResponse(bool isLive, string liveShowName, IEnumerable<UpcomingEvent> events)
        {
            IsLive = isLive;
            LiveShowName = liveShowName ?? Settings.NameOfUntitledLiveShow;

            Events = new List<UpcomingEvent>(events);
        }

        public UpcomingResponse(bool wasSuccessful, string errorMessage)
        {
            WasSuccessful = wasSuccessful;
            ErrorMessage = errorMessage ?? "An error occurred.";
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(IsLive ? "IsLive: true" : "IsLive: false");
            sb.AppendLine(LiveShowName);

            if (Events.Count > 0)
            {
                sb.AppendLine("Events:");

                foreach (UpcomingEvent each in Events)
                {
                    sb.AppendLine(each.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
