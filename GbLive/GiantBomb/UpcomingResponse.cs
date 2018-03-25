using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbLive.GiantBomb
{
    public class UpcomingResponse
    {
        private readonly bool _wasSuccessful = true;
        public bool WasSuccessful => _wasSuccessful;

        private readonly string _errorMessage = string.Empty;
        public string ErrorMessage => _errorMessage;

        private readonly string _liveShowName = Settings.NameOfNoLiveShow;
        public string LiveShowName => _liveShowName;

        private readonly bool _isLive = false;
        public bool IsLive => _isLive;

        private readonly List<UpcomingEvent> _events = default;
        public IReadOnlyList<UpcomingEvent> Events => _events.AsReadOnly();

        public UpcomingResponse(bool isLive, string liveShowName, IEnumerable<UpcomingEvent> events)
        {
            _isLive = isLive;
            _liveShowName = liveShowName ?? Settings.NameOfUntitledLiveShow;

            _events = new List<UpcomingEvent>(events);
        }

        public UpcomingResponse(bool wasSuccessful, string errorMessage)
        {
            _wasSuccessful = wasSuccessful;
            _errorMessage = errorMessage ?? "An error occurred.";
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(IsLive ? "IsLive: true" : "IsLive: false");
            sb.AppendLine(LiveShowName);

            if (_events.Count > 0)
            {
                sb.AppendLine("Events:");

                foreach (UpcomingEvent each in _events)
                {
                    sb.AppendLine(each.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
