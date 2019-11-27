using System.Collections.Generic;
using System.Linq;

namespace GBLive.GiantBomb
{
    public enum Reason
    {
        None,
        Success,
        InternetError,
        TextEmpty,
        ParseFailed
    }

    public class UpcomingResponse
    {
        public Reason Reason { get; set; } = Reason.None;
        public bool IsLive { get; set; } = false;
        public string LiveShowTitle { get; set; } = Settings.NameOfUntitledLiveShow;
        public IEnumerable<UpcomingEvent> Events { get; set; } = Enumerable.Empty<UpcomingEvent>();

        public UpcomingResponse() { }
    }
}