using System;
using System.Windows;

namespace GB_Live
{
    public partial class App : Application
    {
        [STAThread]
        public static int Main(string[] args)
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();

            return 0;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Misc.LogException(e.Exception);

            e.Handled = false;
        }
    }
}
