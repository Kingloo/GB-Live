using System;
using System.Windows;
using System.Windows.Threading;
using GBLive.Common;

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

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                Log.Exception(ex, includeStackTrace: true);
            }
            else
            {
                Log.Message("unhandled exception was empty");
            }
        }
    }
}
