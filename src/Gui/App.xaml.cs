using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace GBLive.Gui
{
	public partial class App : Application
	{
		private static readonly UTF8Encoding utf8Encoding = new UTF8Encoding(
			encoderShouldEmitUTF8Identifier: false, // aka don't print a byte-order mark
			throwOnInvalidBytes: false);

		public App()
		{
			InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindowViewModel viewModel = new MainWindowViewModel();

			MainWindow = new MainWindow(viewModel);

			MainWindow.Show();
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string logfileName = "logfile.txt";

			string logfilePath = Path.Combine(userHomeDirectory, logfileName);

			StringBuilder messageBuilder = CreateExceptionMessage(e.Exception);

			File.AppendAllText(logfilePath, messageBuilder.ToString(), utf8Encoding);

			e.Handled = false;
		}

		private static StringBuilder CreateExceptionMessage(Exception unhandledException)
		{
			StringBuilder messageBuilder = new StringBuilder();

			messageBuilder.Append('[');
			messageBuilder.Append(DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.CurrentCulture));
			messageBuilder.Append(']');

			messageBuilder.Append(' ');
			messageBuilder.Append(GetProgramNameAndProcessId());
			messageBuilder.Append(' ');

			messageBuilder.Append(
				CultureInfo.CurrentCulture,
				$"{unhandledException.GetType().FullName}: {unhandledException.Message}");

			messageBuilder.Append(Environment.NewLine);

			return messageBuilder;
		}

		private static string GetProgramNameAndProcessId()
		{
			string programName = Process.GetCurrentProcess().MainModule?.ModuleName ?? "GBLive";
			int processId = Environment.ProcessId;

			return $"{programName}[{processId}]";
		}
	}
}
