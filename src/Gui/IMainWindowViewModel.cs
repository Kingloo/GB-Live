using System.Collections.Generic;
using System.Threading.Tasks;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.Gui
{
	public interface IMainWindowViewModel
	{
		bool IsLive { get; set; }
		string LiveShowTitle { get; set; }
		IReadOnlyCollection<IShow> Shows { get; }

		Task UpdateAsync();

		void OpenHomePage();
		void OpenChatPage();

		void StartTimer();
		void StopTimer();
	}
}
