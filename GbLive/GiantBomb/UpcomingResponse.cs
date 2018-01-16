using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GbLive.GiantBomb
{
    public class UpcomingResponse
    {
        private readonly string _liveShowName = Settings.NameOfNoLiveShow;
        public string LiveShowName => _liveShowName;

        private readonly bool _isLive = false;
        public bool IsLive => _isLive;

        private readonly List<UpcomingEvent> _events = new List<UpcomingEvent>();
        public IReadOnlyList<UpcomingEvent> Events => _events.AsReadOnly();

        public UpcomingResponse(bool isLive, string liveShowName, IEnumerable<UpcomingEvent> events)
        {
            _isLive = isLive;
            _liveShowName = liveShowName ?? Settings.NameOfUntitledLiveShow;

            if (events.Any())
            {
                foreach (UpcomingEvent each in events)
                {
                    _events.Add(each);
                }
            }
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "IsLive: {0}", IsLive));
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
