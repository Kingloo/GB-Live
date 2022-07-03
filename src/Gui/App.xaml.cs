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
		private const string logSettingsFilename = "LogSettings.json";
		private const string giantBombSettingsFilename = "GiantBombSettings.json";

		public App()
		{
			InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			ILog logger = CreateLogger();
			ISettings giantBombSettings = CreateGiantBombSettings();

			IGiantBombContext gbContext = new GiantBombContext(logger, giantBombSettings);

			IMainWindowViewModel viewModel = new MainWindowViewModel(logger, gbContext, giantBombSettings);

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

		private static ILog CreateLogger()
		{
			LogSettings logSettings = LoadLogSettings();

			string logPath = !String.IsNullOrWhiteSpace(logSettings.Path) ? logSettings.Path : GetDefaultLogFilePath();
			Severity severity = Enum.TryParse(logSettings.Severity, out Severity sev) ? sev : Severity.Warning;

			return new Log(logPath, severity);
		}

		private static LogSettings LoadLogSettings()
		{
			string text = LoadText(logSettingsFilename);

			return JsonSerializer.Deserialize<LogSettings>(text) ?? throw new ArgumentNullException("loading logging settings failed");
		}

		private static string GetDefaultLogFilePath()
		{
			string defaultLogFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string defaultLogFileName = "logfile.txt";

			return Path.Combine(defaultLogFileDirectory, defaultLogFileName);
		}

		private static ISettings CreateGiantBombSettings()
		{
			ISettings settings = LoadGiantBombSettings();

			settings.IsLiveMessage = Constants.IsLiveMessage;
			settings.IsNotLiveMessage = Constants.IsNotLiveMessage;
			settings.NameOfUntitledLiveShow = Constants.NameOfUntitledLiveShow;
			settings.NameOfNoLiveShow = Constants.NameOfNoLiveShow;

			return settings;
		}

		private static ISettings LoadGiantBombSettings()
		{
			string text = LoadText(giantBombSettingsFilename);

			return JsonSerializer.Deserialize<Settings>(text) ?? throw new ArgumentNullException("loading giantbomb settings failed");
		}

		private static string LoadText(string filename)
		{
			string directory = Directory.GetCurrentDirectory();
			string fullPath = Path.Combine(directory, filename);

			return File.ReadAllText(fullPath);
		}
	}
}
