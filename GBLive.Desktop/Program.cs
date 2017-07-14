using System;
using System.Globalization;
using GBLive.Desktop.Common;
using GBLive.Desktop.GiantBomb;

namespace GBLive.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            int exitCode = 0;

            using (var gbService = new GiantBombService())
            {
                var app = new App(gbService);

                exitCode = app.Run();
            }
            
            if (exitCode != 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "exited with code: {0}", exitCode);

                Log.LogMessage(message);
            }
            
            return exitCode;
        }
    }
}
