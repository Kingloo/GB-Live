using System;
using System.Globalization;
using GbLive.Common;

namespace GbLive
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            App app = new App();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "exited - code: {0}", exitCode);

                Log.LogMessage(errorMessage);
            }

            return exitCode;
        }
    }
}
