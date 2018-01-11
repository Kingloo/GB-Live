using System.Windows;
using System.Windows.Threading;
using GbLive.Common;

namespace GbLive
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            Log.LogException(ex.Exception, includeStackTrace: true);
        }
    }
}
