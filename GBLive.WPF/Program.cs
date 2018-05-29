using System;
using System.IO;
using GBLive.WPF.Common;
using GBLive.WPF.GiantBomb;
using GBLive.WPF.GUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace GBLive.WPF
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            JSchema schema = LoadSchema();

            using (var service = schema == null ? new GiantBombService() : new GiantBombService(schema))
            using (var viewModel = new MainWindowViewModel(service, autoStartTimer: true))
            {
                var window = new MainWindow(viewModel);

                window.ShowDialog();
            }

            return 0;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.LogException(ex);
            }
            else
            {
                Log.LogMessage("exited with empty UnhandledException");
            }
        }

        private static JSchema LoadSchema()
        {
            try
            {
                using (var stream = File.OpenText(@"GiantBomb\schema.json"))
                using (var reader = new JsonTextReader(stream))
                {
                    return JSchema.Load(reader);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
