using System;
using System.IO;
using System.Windows;

namespace GB_Live
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Misc.LogException(e.Exception);
        }
    }
}
