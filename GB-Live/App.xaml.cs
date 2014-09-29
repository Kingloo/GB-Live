using System;
using System.Windows;

namespace GB_Live
{
    public partial class App : Application
    {
        public static Uri gbHome = new Uri("http://www.giantbomb.com/");
        public static Uri gbChat = new Uri("http://www.giantbomb.com/chat");
        
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
