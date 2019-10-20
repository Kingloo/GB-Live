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
            MainWindowViewModel vm = new MainWindowViewModel();

            MainWindow = new MainWindow(vm);

            MainWindow.Show();
        }
    }
}
