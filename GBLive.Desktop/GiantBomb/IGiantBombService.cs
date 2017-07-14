using System.Collections.Generic;
using System.Threading.Tasks;

namespace GBLive.Desktop.GiantBomb
{
    public interface IGiantBombService
    {
        bool IsLive { get; }
        string LiveShowName { get; }
        IReadOnlyCollection<UpcomingEvent> Events { get; }

        Task UpdateAsync();
    }
}