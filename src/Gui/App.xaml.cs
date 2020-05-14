using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using GBLive.Common;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.Gui
{
    public partial class App : Application
    {
        public App() { }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ILog logger = new Log(GetDefaultLogFilePath(), Severity.Warning);

            ISettings settings = LoadSettings();
            
            settings.IsLiveMessage              = Constants.IsLiveMessage;
            settings.IsNotLiveMessage           = Constants.IsNotLiveMessage;
            settings.NameOfUntitledLiveShow     = Constants.NameOfUntitledLiveShow;
            settings.NameOfNoLiveShow           = Constants.NameOfNoLiveShow;

            IGiantBombContext gbContext = new GiantBombContext(logger, settings);

            IMainWindowViewModel viewModel = new MainWindowViewModel(logger, gbContext, settings);

            MainWindow = new MainWindow(logger, viewModel);

            MainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                LogStatic.Exception(ex, includeStackTrace: true);
            }
            else
            {
                LogStatic.Message("unhandled exception was empty");
            }
        }

        private static string GetDefaultLogFilePath()
        {
            string defaultLogFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string defaultLogFileName = "logfile.txt";

            return Path.Combine(defaultLogFileDirectory, defaultLogFileName);
        }

        private static ISettings LoadSettings()
        {
            string directory = Directory.GetCurrentDirectory();
            string filename = "GiantBombSettings.json";

            string path = Path.Combine(directory, filename);

            string raw = File.ReadAllText(path);

            return JsonSerializer.Deserialize<Settings>(raw);
        }
    }
}
