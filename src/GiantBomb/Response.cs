using System.Collections.Generic;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.GiantBomb
{
    public class Response : IResponse
    {
        public Reason Reason { get; set; } = Reason.None;
        public bool IsLive { get; set; } = false;
        public string LiveShowTitle { get; set; } = string.Empty;
        public IReadOnlyCollection<IShow> Shows { get; set; } = new List<IShow>().AsReadOnly();

        public Response() { }
    }
}
