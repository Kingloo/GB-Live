using System;
using System.Collections.Generic;

namespace GBLive.WPF.GiantBomb
{
    public enum Reason
    {
        None,
        Success,
        InternetError,
        StringEmpty,
        ParseFailed,
        ValidateFailed
    }

    public class UpcomingResponse
    {
        public bool IsSuccessful { get; } = false;
        public Reason Reason { get; } = Reason.None;
        public bool IsLive { get; } = false;

        private string _liveShowName = Settings.NameOfNoLiveShow;
        public string LiveShowName
        {
            get => _liveShowName;
            set => _liveShowName = String.IsNullOrEmpty(value) ? Settings.NameOfUntitledLiveShow : value;
        }

        private IReadOnlyList<UpcomingEvent> _events = new List<UpcomingEvent>();
        public IReadOnlyList<UpcomingEvent> Events
        {
            get => _events;
            set => _events = value ?? new List<UpcomingEvent>();
        }

        public UpcomingResponse() { }

        public UpcomingResponse(bool isSuccessful, Reason reason, bool isLive)
        {
            IsSuccessful = isSuccessful;
            Reason = reason;
            IsLive = isLive;
        }
    }
}
