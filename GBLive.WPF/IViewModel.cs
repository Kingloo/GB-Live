using System.Collections.Generic;
using System.Threading.Tasks;

namespace GBLive.WPF
{
    public interface IViewModel
    {
        bool IsLive { get; }
        string LiveShowName { get; }
        IReadOnlyCollection<GBUpcomingEvent> Events { get; }

        void GoToHomePage();
        void GoToChatPage();
        void StartUpdateTimer();

        Task UpdateAsync();
    }
}
