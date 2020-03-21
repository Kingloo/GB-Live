using System.Collections.Generic;

namespace GBLive.GiantBomb.Interfaces
{
    public interface IResponse
    {
        Reason Reason { get; set; }
        bool IsLive { get; set; }
        string LiveShowTitle { get; set; }
        IReadOnlyCollection<IShow> Shows { get; set; }
    }
}
