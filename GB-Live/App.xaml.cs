using System.Windows;
using System.Windows.Threading;

namespace GB_Live
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.LogException(e.Exception);
        }
    }
}
