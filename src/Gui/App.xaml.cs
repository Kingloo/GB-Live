using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using GBLive.Common;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GBLive.Gui
{
    public partial class App : Application
    {
        private readonly IHost host;

        public App()
        {
            host = BuildGenericHost();

            InitializeComponent();
        }

        private static IHost BuildGenericHost()
        {
            return new HostBuilder()
                .ConfigureAppConfiguration((ctx, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                    }
                )
                .ConfigureServices((ctx, services) =>
                    {
                        services.Configure<Settings>(settings =>
                        {
                            settings.IsLiveMessage = Constants.IsLiveMessage;
                            settings.IsNotLiveMessage = Constants.IsNotLiveMessage;
                            settings.NameOfNoLiveShow = Constants.NameOfNoLiveShow;
                            settings.NameOfUntitledLiveShow = Constants.NameOfUntitledLiveShow;

                            settings.UserAgent = ctx.Configuration.GetValue<string>("GiantBomb:UserAgent");
                            settings.Home = ctx.Configuration.GetValue<Uri>("GiantBomb:Home");
                            settings.Chat = ctx.Configuration.GetValue<Uri>("GiantBomb:Chat");
                            settings.Upcoming = ctx.Configuration.GetValue<Uri>("GiantBomb:Upcoming");
                            settings.FallbackImage = ctx.Configuration.GetValue<Uri>("GiantBomb:FallbackImage");
                            settings.UpdateIntervalInSeconds = ctx.Configuration.GetValue<double>("GiantBomb:UpdateIntervalInSeconds");
                            settings.ShouldNotify = ctx.Configuration.GetValue<bool>("GiantBomb:ShouldNotify");
                        });

                        string logFilePath = ctx.Configuration.GetValue<string>("ILogClass:Path");

                        if (String.IsNullOrWhiteSpace(logFilePath))
                        {
                            logFilePath = DefaultLogFilePath();
                        }

                        Severity severity = ctx.Configuration.GetValue<Severity>("ILogClass:Severity");

                        ILogClass myLogger = new LogClass(logFilePath, severity);

                        services.AddSingleton(typeof(ILogClass), myLogger);

                        services.AddTransient<IGiantBombContext, GiantBombContext>();
                        services.AddTransient<IMainWindowViewModel, MainWindowViewModel>();

                        services.AddSingleton<MainWindow>();
                    }
                )
                .Build();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await host.StartAsync();

            MainWindow = host.Services.GetRequiredService<MainWindow>();

            MainWindow.Show();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                LogStatic.Exception(ex, includeStackTrace: true);
            }
            else
            {
                LogStatic.Message("unhandled exception was empty", Severity.Error);
            }
        }

        private static string DefaultLogFilePath()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filename = "logfile.txt";

            return Path.Combine(homeDir, filename);
        }
    }
}
