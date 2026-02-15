# Architecture: SCTools-Modern

## Overview

Clean-ish MVVM: принципы Clean Architecture (разделение через интерфейсы) без избыточной церемонии. Три проекта — минимум, достаточный для ~12K строк.

## Project Dependencies

```
SCTools.App (WPF)
    |
    +---> SCTools.Core (netstandard/net10.0)
    |
SCTools.Tests (xUnit)
    |
    +---> SCTools.Core
    +---> SCTools.App (ViewModels only)
```

**SCTools.Core** — zero UI dependencies. Чистая .NET class library.
**SCTools.App** — WPF-специфичный код: Views, ViewModels, DI bootstrap.
**SCTools.Tests** — тесты для Core services и App ViewModels.

## Solution Structure

```
SCTools.sln
+-- Directory.Build.props          # Shared build properties
+-- .editorconfig                  # Code style rules
+-- src/
|   +-- SCTools.App/               # WPF Application
|   |   +-- App.xaml(.cs)          # DI container, startup, Serilog
|   |   +-- Views/
|   |   |   +-- MainWindow.xaml
|   |   |   +-- SettingsView.xaml
|   |   |   +-- LocalizationView.xaml
|   |   |   +-- Dialogs/
|   |   |       +-- DownloadProgressDialog.xaml
|   |   +-- ViewModels/
|   |   |   +-- MainWindowViewModel.cs
|   |   |   +-- SettingsViewModel.cs
|   |   |   +-- LocalizationViewModel.cs
|   |   |   +-- DownloadProgressViewModel.cs
|   |   +-- Converters/            # IValueConverter implementations
|   |   +-- Resources/             # Styles, Icons, Strings (.resx)
|   |   +-- Helpers/               # WPF-specific utilities
|   |   +-- SCTools.App.csproj
|   |
|   +-- SCTools.Core/              # Business Logic Library
|       +-- Models/
|       |   +-- GameMode.cs        # enum: LIVE, PTU, EPTU
|       |   +-- GameInstallation.cs
|       |   +-- LanguagePack.cs
|       |   +-- AppSettings.cs
|       +-- Services/
|       |   +-- Interfaces/
|       |   |   +-- IGitHubApiService.cs
|       |   |   +-- ILanguagePackService.cs
|       |   |   +-- IGameConfigService.cs
|       |   |   +-- IAutoUpdateService.cs
|       |   +-- GitHubApiService.cs
|       |   +-- LanguagePackService.cs
|       |   +-- GameConfigService.cs
|       |   +-- AutoUpdateService.cs
|       +-- Exceptions/
|       |   +-- GitHubRateLimitException.cs
|       |   +-- PackageVerificationException.cs
|       +-- SCTools.Core.csproj
|
+-- tests/
    +-- SCTools.Tests/
        +-- ViewModels/
        +-- Services/
        +-- TestHelpers/
        +-- SCTools.Tests.csproj
```

## DI Registration (App.xaml.cs)

```csharp
var host = Host.CreateDefaultBuilder()
    .UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.File("logs/sctools-.log", rollingInterval: RollingInterval.Day))
    .ConfigureServices((ctx, services) =>
    {
        // Configuration
        services.Configure<AppSettings>(ctx.Configuration.GetSection("App"));

        // HTTP
        services.AddHttpClient<IGitHubApiService, GitHubApiService>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SCTools/2.0");
        });

        // Services
        services.AddSingleton<ILanguagePackService, LanguagePackService>();
        services.AddSingleton<IGameConfigService, GameConfigService>();
        services.AddSingleton<IAutoUpdateService, AutoUpdateService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<LocalizationViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    })
    .Build();
```

## Key Patterns

### MVVM with CommunityToolkit.Mvvm

```csharp
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IGitHubApiService _gitHubApi;

    [ObservableProperty]
    private GameMode _selectedGameMode = GameMode.LIVE;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task RefreshReleasesAsync(CancellationToken token)
    {
        IsLoading = true;
        try { /* ... */ }
        finally { IsLoading = false; }
    }
}
```

### Async in Desktop App

| Layer | ConfigureAwait(false) | Why |
|-------|----------------------|-----|
| ViewModel | NO | Must return to UI thread for bindings |
| Service | YES, always | Avoid deadlocks, improve throughput |
| Infrastructure | YES, always | No UI dependency |

### Error Handling

- Services throw typed exceptions (GitHubRateLimitException, etc.)
- ViewModels catch and display via IDialogService
- Unhandled exceptions logged by Serilog + shown to user

### Security

- Tokens: Windows Credential Manager (never plaintext JSON)
- TLS: System defaults (no hardcoded versions)
- Paths: `Path.GetFullPath()` + prefix validation
- URLs: `Uri.TryCreate()` + scheme allowlist before Process.Start()
- NuGet audit: `<NuGetAuditMode>all</NuGetAuditMode>` with NU1901-NU1904 as errors
