using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FileLogger;
using GBLive.Exceptions;
using GBLive.Options;
using static GBLive.EventIds.App;
using static GBLive.Gui.AppLoggerMessages;

namespace GBLive.Gui
{
#pragma warning disable IDE0058
	public partial class App : Application
	{
		private readonly IHost host;
		private readonly ILogger<App> logger;

		public App()
		{
			InitializeComponent();

			host = BuildHost();

			logger = host.Services.GetRequiredService<ILogger<App>>();
		}

		private static IHost BuildHost()
		{
			return new HostBuilder()
				.ConfigureHostConfiguration(ConfigureHostConfiguration)
				.ConfigureLogging(ConfigureLogging)
				.ConfigureHostOptions(ConfigureHostOptions)
				.ConfigureAppConfiguration(ConfigureAppConfiguration)
				.ConfigureServices(ConfigureServices)
				.Build();
		}

		private static void ConfigureHostConfiguration(IConfigurationBuilder builder)
		{
			builder
				.AddCommandLine(Environment.GetCommandLineArgs())
				.AddEnvironmentVariables();
		}

		private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
		{
			builder.AddConfiguration(context.Configuration.GetSection("Logging"));

			if (Debugger.IsAttached)
			{
				builder.AddDebug();
			}

			builder.AddEventLog();

			builder.AddFileLogger();
		}

		private static void ConfigureHostOptions(HostBuilderContext context, HostOptions options)
		{
			options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
			options.ShutdownTimeout = TimeSpan.FromSeconds(5d);
		}

		private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
		{
			if (!context.HostingEnvironment.IsProduction())
			{
				string environmentMessage = $"environment is {context.HostingEnvironment.EnvironmentName}";

				Console.Out.WriteLine(environmentMessage);
				Console.Error.WriteLine(environmentMessage);
				Debug.WriteLineIf(Debugger.IsAttached, environmentMessage);
			}

			builder.SetBasePath(AppContext.BaseDirectory);

			const string permanent = "appsettings.json";
			string environment = $"appsettings.{context.HostingEnvironment.EnvironmentName}.json";

			builder
				.AddJsonFile(permanent, optional: false, reloadOnChange: true)
				.AddJsonFile(environment, optional: true, reloadOnChange: true);
		}

		private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			services.AddSingleton<IFileLoggerSink, FileLoggerSink>();

			services.Configure<GBLiveOptions>(context.Configuration.GetSection("GBLiveOptions"));
			services.Configure<GiantBombOptions>(context.Configuration.GetSection("GiantBombOptions"));

			services.AddTransient<MainWindowViewModel>();
			services.AddTransient<MainWindow>();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			LogStartupStarted(logger);

			host.Start();

			IHostApplicationLifetime appLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

			IFileLoggerSink sink = host.Services.GetRequiredService<IFileLoggerSink>();

			sink.StartSink();

			using (host.Services.CreateScope())
			{
				MainWindow = host.Services.GetRequiredService<MainWindow>();
			}

			appLifetime.ApplicationStopping.Register(() =>
			{
				MainWindow?.Close();
			},
			useSynchronizationContext: true);

			MainWindow.Show();

			LogStartupFinished(logger);
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (e.ApplicationExitCode == 0)
			{
				LogAppExitZero(logger);
			}
			else
			{
				LogAppExitNotZero(logger, e.ApplicationExitCode);
			}

			IFileLoggerSink sink = host.Services.GetRequiredService<IFileLoggerSink>();

			sink.StopSink();

			host.StopAsync().GetAwaiter().GetResult();

			host.Dispose();
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Exception toLog = e.Exception switch
			{
				TargetInvocationException ex => ex.InnerException is not null
					? ex.InnerException
					: new GBLiveException("DispatcherUnhandledException is TargetInvocationException with null inner exception"),
				Exception ex => ex,
				_ => new GBLiveException("DispatcherUnhandledException thrown but .Exception was null")
			};

			LogDispatcherUnhandledException(logger, toLog.GetType().Name, toLog.Message);
		}
	}

	internal static partial class AppLoggerMessages
	{
		[LoggerMessage(StartupStartedId, LogLevel.Debug, "startup started")]
		internal static partial void LogStartupStarted(ILogger<App> logger);

		[LoggerMessage(StartupFinishedId, LogLevel.Information, "started")]
		internal static partial void LogStartupFinished(ILogger<App> logger);

		[LoggerMessage(AppExitZeroId, LogLevel.Information, "exited")]
		internal static partial void LogAppExitZero(ILogger<App> logger);

		[LoggerMessage(AppExitNotZeroId, LogLevel.Critical, "exited (exit code {ExitCode})")]
		internal static partial void LogAppExitNotZero(ILogger<App> logger, int exitCode);

		[LoggerMessage(DispatcherUnhandledExceptionId, LogLevel.Error, "unhandled exception: '{Type}' with message: '{Message}'")]
		internal static partial void LogDispatcherUnhandledException(ILogger<App> logger, string type, string message);
	}
#pragma warning restore IDE0058
}
