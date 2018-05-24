using System;
using System.IO;
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
            JSchema schema = LoadSchema();

            using (var service = schema == null ? new GiantBombService() : new GiantBombService(schema))
            using (var viewModel = new MainWindowViewModel(service, autoStartTimer: true))
            {
                var window = new MainWindow(viewModel);

                window.ShowDialog();
            }

            return 0;
        }

        private static JSchema LoadSchema()
        {
            try
            {
                var stream = File.OpenText(@"GiantBomb\schema.json");
                var reader = new JsonTextReader(stream);

                return JSchema.Load(reader);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
