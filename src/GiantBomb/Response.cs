using System.Collections.Generic;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.GiantBomb
{
    public class Response : IResponse
    {
        public Reason Reason { get; set; } = Reason.None;
        public LiveNowData? LiveNow { get; set; }
        public IReadOnlyCollection<IShow> Shows { get; set; } = new List<IShow>().AsReadOnly();

        public Response() { }
    }
}
