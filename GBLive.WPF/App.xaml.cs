using System;
using System.Windows;
using System.Windows.Threading;
using GBLive.Desktop.Common;
using GBLive.Desktop.GiantBomb;
using GBLive.Desktop.Views;

namespace GBLive.Desktop
{
    public partial class App : Application
    {
        public App(IGiantBombService gbService)
        {
            if (gbService == null) { throw new ArgumentNullException(nameof(gbService)); }

            InitializeComponent();

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            MainWindow = new MainWindow(new MainWindowViewModel(gbService, autoStart: true));
            MainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            Log.LogException(ex.Exception);
        }
    }
}
