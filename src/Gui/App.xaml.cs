using System.Windows;

namespace GBLive.Gui
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindowViewModel viewModel = new MainWindowViewModel();

			MainWindow = new MainWindow(viewModel);

			MainWindow.Show();
		}
	}
}
