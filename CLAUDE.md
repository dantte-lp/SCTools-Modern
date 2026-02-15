# Project: SCTools-Modern

Утилита-компаньон для Star Citizen: управление локализациями, настройками игры и авто-обновлениями.
Миграция с .NET Framework 4.5 (WinForms) на .NET 10 (WPF + Fluent) с MVVM-архитектурой.

## Tech Stack

- **Runtime:** .NET 10 LTS, C# 14, x64-only, self-contained deployment
- **UI:** WPF + **WPF UI 4.2.0** (wpfui.lepo.co) — FluentWindow, NavigationView, Mica/Acrylic
- **MVVM:** CommunityToolkit.Mvvm 8.4.0 (source generators: [ObservableProperty], [RelayCommand])
- **DI/Config:** Microsoft.Extensions.Hosting 10.0.2 + Microsoft.Extensions.Http 10.0.2
- **Logging:** Serilog.Extensions.Hosting 10.0.0, Serilog.Sinks.File 7.0.0, Serilog.Sinks.Debug 3.0.0
- **JSON:** System.Text.Json (source generators, NO Newtonsoft)
- **HTTP:** IHttpClientFactory (Microsoft.Extensions.Http 10.0.2)
- **Updates:** Velopack 0.0.1298 (delta updates via GitHub Releases)
- **Secrets:** Meziantou.Framework.Win32.CredentialManager 1.7.11 (Windows Credential Manager)
- **Hashing:** System.IO.Hashing 10.0.2 (xxHash3) + SHA256 for verification
- **Archives:** SharpCompress 0.41.0 (7z/tar.gz support)
- **GitHub API:** Octokit 14.0.0
- **System Tray:** H.NotifyIcon.Wpf 2.4.1
- **Tests:** xUnit v3 3.2.2 + NSubstitute 5.3.0 + FluentAssertions 8.8.0
- **Analyzers:** StyleCop.Analyzers 1.2.0-beta.556, Microsoft.CodeAnalysis.NetAnalyzers 10.0.101

## Container Development (Podman)

Build and test inside a Podman OCI container (Debian trixie, .NET 10 SDK):

```bash
podman build -t sctools-dev -f Containerfile .   # Build dev image
podman run --rm -v ./:/workspace:Z sctools-dev dotnet build src/SCTools.Core/ -c Release
podman run --rm -v ./:/workspace:Z sctools-dev dotnet test tests/SCTools.Tests/ -c Release
podman run --rm -v ./:/workspace:Z sctools-dev dotnet format src/SCTools.Core/ --verify-no-changes --severity warn
```

**Cross-platform split:** Core targets `net10.0` (builds on Linux), App targets `net10.0-windows` (Windows only, WPF). ViewModels live in Core (CommunityToolkit.Mvvm is cross-platform). Tests auto-detect OS; WPF integration tests excluded on Linux.

## Key Commands

```bash
dotnet build src/SCTools.Core/ -c Release        # Build Core (cross-platform)
dotnet test tests/SCTools.Tests/ -c Release       # Tests (Core on Linux, all on Windows)
dotnet format --verify-no-changes --severity warn # Format check
dotnet build -c Release -warnaserror              # Strict build (Windows only, full solution)
```

## Architecture (3 projects, Clean-ish MVVM)

- `src/SCTools.App/` — WPF UI: Views (XAML), DI setup (App.xaml.cs). **net10.0-windows only.**
- `src/SCTools.Core/` — Pure .NET library: Models, Services, ViewModels, Helpers. **net10.0 (cross-platform).**
- `tests/SCTools.Tests/` — xUnit tests. All tests (including ViewModels) run everywhere; WPF integration tests Windows-only.

## Async Rules

- **ViewModels:** NO ConfigureAwait(false). Continuation must return to UI thread.
- **Services (Core):** ALWAYS ConfigureAwait(false) on every await.
- **NEVER** call .Result or .Wait() from UI thread.
- **async void** ONLY in event handlers, wrapping async Task methods.

## Code Standards (STRICT)

- All warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- .NET analyzers on max: `<AnalysisLevel>latest-all</AnalysisLevel>`
- Nullable reference types enabled: `<Nullable>enable</Nullable>`
- StyleCop.Analyzers for style enforcement
- DO NOT format code manually — hooks handle formatting automatically
- All public APIs must have XML documentation (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)
- Use primary constructors, file-scoped namespaces, pattern matching
- Core: `net10.0` (cross-platform), App: `net10.0-windows` (WPF)

## WPF UI Patterns (wpfui.lepo.co)

- Main window: `ui:FluentWindow` with `WindowBackdropType="Mica"` and `ExtendsContentIntoTitleBar="True"`
- Navigation: `ui:NavigationView` with `NavigationViewItem` items, page-based routing
- Pages implement `INavigableView<TViewModel>`, ViewModels implement `INavigationAware`
- DI: `services.AddNavigationViewPageProvider()` + register IThemeService, INavigationService, ISnackbarService
- XAML namespace: `xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"`
- Context7 library ID: `/lepoco/wpfui` — use for up-to-date component docs

## MCP Integration

Use MCP Context7 for up-to-date docs on any library:
1. `resolve-library-id` to find library ID (e.g., `/lepoco/wpfui`, `/xunit/xunit`)
2. `query-docs` to get current API documentation
Available MCP servers: Context7 (library docs), Greptile (code review). See `.mcp.json`.

## Workflow: Plan First, Then Code

1. **Always plan before coding.** Use Plan Mode (Shift+Tab x2) for non-trivial tasks.
2. Break work into sprints (Agile). Each sprint = 1 feature or module.
3. Write tests alongside implementation (TDD encouraged).
4. Run analyzers after every change — zero warnings policy.
5. Commit atomically: one feature = one commit.

## Sprint Backlog (Migration Order)

See @docs/SPRINT_PLAN.md for full Agile backlog.
See @docs/architecture.md for detailed architecture.
See @docs/migration-checklist.md for file-by-file migration map.

## Security

- NEVER hardcode secrets, tokens, or API keys
- Use Windows Credential Manager for all sensitive data
- HTTPS only for all network requests; no TLS version hardcoding
- Validate all file paths against traversal (Path.GetFullPath + prefix check)
- Validate URLs before Process.Start (Uri.TryCreate, scheme allowlist)

## Compact Instructions

When compacting, preserve: test results, current file changes, architecture decisions, sprint progress.
Discard: verbose command output, intermediate failed attempts.
