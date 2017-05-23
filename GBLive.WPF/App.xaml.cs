using System;
using System.Windows;
using System.Windows.Threading;
using GBLive.WPF.Common;

namespace GBLive.WPF
{
    public partial class App : Application
    {
        public App(GBSettings settings, IWebRetriever jsonRetriever)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
            if (jsonRetriever == null) { throw new ArgumentNullException(nameof(jsonRetriever)); }

            InitializeComponent();

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            MainWindow = new MainWindow(new MainWindowViewModel(settings, jsonRetriever, true));
            MainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            Log.LogException(ex.Exception);
        }
    }
}
