// Licensed to the SCTools project under the MIT license.

using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SCTools.App.Services;
using SCTools.App.ViewModels;
using SCTools.App.Views.Pages;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using SCTools.Core.ViewModels;
using Serilog;
using Serilog.Events;
using Velopack;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace SCTools.App;

/// <summary>
/// Application entry point with DI container, Serilog logging, and Velopack auto-updates.
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
        .ConfigureServices((context, services) =>
        {
            // Configuration
            services.Configure<AppSettings>(
                context.Configuration.GetSection(AppSettings.SectionName));

            // Core services
            services.AddSingleton<IFileSystem, PhysicalFileSystem>();
            services.AddSingleton<IGameDetectionService, GameDetectionService>();
            services.AddSingleton<IGameConfigService, GameConfigService>();
            services.AddSingleton<ILanguagePackService, LanguagePackService>();
            services.AddSingleton<IFileIndexService, FileIndexService>();

            // HTTP + GitHub API
            services.AddSingleton<Octokit.IGitHubClient>(_ =>
                new Octokit.GitHubClient(new Octokit.ProductHeaderValue("SCTools", "2.0")));
            services.AddHttpClient<IGitHubApiService, GitHubApiService>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SCTools/2.0");
            });

            // Localization updater (depends on IGitHubApiService, ILanguagePackService)
            services.AddSingleton<ILocalizationUpdater, LocalizationUpdater>();

            // Auto-update (Velopack adapter)
            services.AddSingleton<IUpdateManagerAdapter>(_ =>
                new VelopackUpdateManagerAdapter(new Uri("https://github.com/dantte-lp/SCTools-Modern")));
            services.AddSingleton<IAutoUpdateService, AutoUpdateService>();

            // Core ViewModels (singletons for shared state)
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LocalizationViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<DownloadProgressViewModel>();

            // App ViewModels
            services.AddSingleton<ShellViewModel>();

            // WPF UI services
            services.AddNavigationViewPageProvider();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();

            // Main window + pages
            services.AddSingleton<INavigationWindow, MainWindow>();
            services.AddSingleton<LocalizationPage>();
            services.AddSingleton<SettingsPage>();
        })
        .Build();

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Runs Velopack update hooks before any UI code.
    /// </summary>
    public App()
    {
        VelopackApp.Build().Run();
    }

    /// <summary>
    /// Gets the application-wide service provider.
    /// </summary>
    public static IServiceProvider Services => AppHost.Services;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await AppHost.StartAsync();

        // Initialize MainWindowViewModel with settings
        var mainVm = AppHost.Services.GetRequiredService<MainWindowViewModel>();
        var settings = AppHost.Services
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value;
        mainVm.Initialize(settings);

        var navigationWindow = AppHost.Services.GetRequiredService<INavigationWindow>();
        navigationWindow.ShowWindow();
        navigationWindow.Navigate(typeof(LocalizationPage));
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
