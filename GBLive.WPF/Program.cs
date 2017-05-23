using System;
using System.Diagnostics;
using System.Globalization;
using GBLive.WPF.Common;

namespace GBLive.WPF
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            var settings = new GBSettings();

            var jsonRetriever = new SimpleWebRetriever(settings.GBJson)
            {
                UserAgent = settings.UserAgent
            };
            
            App app = new App(settings, jsonRetriever);

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "{0} exited with code: {1}", Process.GetCurrentProcess().MainModule.ModuleName, exitCode);

                Log.LogMessage(message);
            }

            return exitCode;
        }
    }
}
