using System;
using System.Globalization;

namespace GB_Live
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            App app = new App();
            app.InitializeComponent();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "Exited with code {0}", exitCode);

                Utils.LogMessage(errorMessage);
            }

            return exitCode;
        }
    }
}
