using System.Collections.Generic;
using System.Threading.Tasks;
using GBLive.Desktop.GiantBomb;

namespace GBLive.Desktop.Views
{
    public interface IViewModel
    {
        bool IsLive { get; }
        string LiveShowName { get; }
        IReadOnlyCollection<UpcomingEvent> Events { get; }

        void GoToHomepage();
        void GoToChatPage();
        
        Task UpdateAsync();
    }
}
