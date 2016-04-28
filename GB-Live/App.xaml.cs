using System;
using System.Windows;

namespace GB_Live
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Utils.LogException(e.Exception);
        }
    }
}
