// Licensed to the SCTools project under the MIT license.

using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SCTools.App.ViewModels;
using SCTools.App.Views.Pages;
using Serilog;
using Serilog.Events;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace SCTools.App;

/// <summary>
/// Application entry point with DI container and Serilog logging.
/// </summary>
public partial class App : Application
{
    private static readonly IHost AppHost = Host
        .CreateDefaultBuilder()
        .UseSerilog((_, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.File(
                "logs/sctools-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14))
        .ConfigureServices((_, services) =>
        {
            // WPF UI services
            services.AddNavigationViewPageProvider();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();

            // Main window
            services.AddSingleton<INavigationWindow, MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            // Pages
            services.AddSingleton<DashboardPage>();
            services.AddSingleton<DashboardViewModel>();
        })
        .Build();

    /// <summary>
    /// Gets the application-wide service provider.
    /// </summary>
    public static IServiceProvider Services => AppHost.Services;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await AppHost.StartAsync();

        var navigationWindow = AppHost.Services.GetRequiredService<INavigationWindow>();
        navigationWindow.ShowWindow();
        navigationWindow.Navigate(typeof(DashboardPage));
    }

    /// <inheritdoc />
    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        await Log.CloseAndFlushAsync();

        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled dispatcher exception");
    }
}
