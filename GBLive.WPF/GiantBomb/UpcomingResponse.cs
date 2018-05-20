using System;
using System.Collections.Generic;

namespace GBLive.WPF.GiantBomb
{
    public enum Reason
    {
        None,
        Success,
        ErrorCode,
        StringEmpty,
        ParseFailed
    }

    public class UpcomingResponse
    {
        public bool IsSuccessful { get; } = false;
        public Reason Reason { get; } = Reason.None;
        public bool IsLive { get; } = false;
        public IReadOnlyList<UpcomingEvent> Events { get; } = new List<UpcomingEvent>();

        public UpcomingResponse() { }

        public UpcomingResponse(bool isSuccessful, Reason reason, bool isLive)
            : this(isSuccessful, reason, isLive, new List<UpcomingEvent>())
        { }

        public UpcomingResponse(bool isSuccessful, Reason reason, bool isLive, IList<UpcomingEvent> events)
        {
            IsSuccessful = isSuccessful;
            Reason = reason;
            IsLive = isLive;
            Events = (IReadOnlyList<UpcomingEvent>)events ?? throw new ArgumentNullException(nameof(events));
        }
    }
}
