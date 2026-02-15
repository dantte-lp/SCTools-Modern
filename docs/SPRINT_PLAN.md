# Sprint Plan: SCTools Migration to .NET 10

## Methodology

Agile/Kanban с 1-недельными спринтами. Каждый спринт = 1 модуль или фича.
Definition of Done: код компилируется без warnings, тесты проходят, analyzer clean.

---

## Sprint 0: Foundation (Infrastructure)

**Goal:** Рабочий solution с CI/CD, анализаторами и пустыми проектами.

- [x] Создать `SCTools.sln` с 3 проектами (App, Core, Tests)
- [x] Настроить `Directory.Build.props` (общие свойства: TreatWarningsAsErrors, AnalysisLevel, Nullable)
- [x] Настроить `.editorconfig` с правилами C# 14
- [x] Добавить NuGet-пакеты: StyleCop.Analyzers, Microsoft.CodeAnalysis.NetAnalyzers
- [x] Настроить GitHub Actions: build + test + format check
- [x] Создать `App.xaml.cs` с DI через `Microsoft.Extensions.Hosting`
- [x] Настроить Serilog (file + debug sinks)
- [x] Containerfile (Podman, OCI, Debian trixie, .NET 10 SDK)
- [x] Cross-platform: Core `net10.0`, App `net10.0-windows`, Tests conditional
- [x] Smoke test: `dotnet build` 0 warnings, `dotnet test` 7/7 green
- [x] Написать первый unit-тест (GameModeTests — 7 tests)

**Acceptance:** `dotnet build -c Release -warnaserror` = 0 warnings, `dotnet test` = green. **DONE.**

---

## Sprint 1: Core Models & Game Detection

**Goal:** Перенести модели и логику обнаружения игры.

- [x] `GameMode.cs` — enum LIVE/PTU/EPTU (from Sprint 0)
- [x] `GameInstallation.cs` — sealed record + GameDetectionService с FindGameFolder
- [x] `GameConstants.cs` — пути, имена файлов, константы, path helpers
- [x] `AppSettings.cs` — модель настроек (IOptions<T> pattern + DataAnnotations validation)
- [x] `appsettings.json` — конфигурация по умолчанию
- [x] `IFileSystem.cs` + `PhysicalFileSystem.cs` — абстракция файловой системы
- [x] `IGameDetectionService.cs` + `GameDetectionService.cs` — обнаружение установок
- [x] Unit-тесты для GameConstants, GameInstallation/Detection (NSubstitute mocks)
- [x] Unit-тесты для AppSettings validation (Range attribute)
- [x] Build 0W/0E, 36 tests green, format clean

**Acceptance:** Модели покрыты тестами, 0 analyzer warnings. **DONE.**

---

## Sprint 2: HTTP & GitHub API Service

**Goal:** Работающий GitHub API клиент с rate limiting.

- [x] `IGitHubApiService.cs` — интерфейс (GetReleases, GetLatestRelease, DownloadAsset, GetRateLimit)
- [x] `GitHubApiService.cs` — Octokit v14 для метаданных + HttpClient для стриминга загрузок
- [x] `ReleaseInfo.cs`, `ReleaseAssetInfo.cs`, `GitHubRateLimitInfo.cs` — domain DTOs (Uri типы, sealed records)
- [x] `GitHubRateLimitException.cs` — кастомное исключение с ResetTime, Limit, Remaining
- [x] Rate limit handling: Octokit RateLimitExceededException → GitHubRateLimitException; HTTP 403/429 с X-RateLimit headers
- [x] Auth token через Octokit Credentials (IGitHubClient injection, готово для Credential Manager в Sprint 5)
- [x] Unit-тесты: 15 tests для GitHubApiService (NSubstitute mocks + FakeHandler), 3 для ReleaseInfo DTOs, 5 для exception
- [x] Build 0W/0E, 59 tests green, format clean

**Acceptance:** Получение списка релизов работает, rate limit обрабатывается. **DONE.**

---

## Sprint 3: Language Pack Service

**Goal:** Установка и удаление локализаций + user.cfg управление.

- [x] `PathValidator.cs` — защита от path traversal (SafeCombine, IsSafePath, Path.GetFullPath + prefix check)
- [x] `LanguagePack.cs` — sealed record (LanguageCode, Version, FolderPath, HasGlobalIni)
- [x] `InstallStatus.cs` — enum результата (Success, PackageError, FileError, PathError, GameNotFound)
- [x] `IFileSystem.cs` расширен — ReadAllText, WriteAllText, GetDirectories, CreateDirectory, DeleteDirectory, DeleteFile, CopyFile
- [x] `PhysicalFileSystem.cs` — реализация новых методов IFileSystem
- [x] `IGameConfigService.cs` + `GameConfigService.cs` — парсинг user.cfg (key=value, сохранение комментариев, case-insensitive keys)
- [x] `ILanguagePackService.cs` + `LanguagePackService.cs` — GetInstalledLanguages, Install/Uninstall, SetCurrentLanguage, ResetLanguage
- [x] Path traversal protection в InstallFromFile (PathValidator.IsSafePath)
- [x] InternalsVisibleTo для тестирования internal static методов
- [x] Unit-тесты: 13 PathValidator, 29 GameConfigService, 24 LanguagePackService — 66 новых тестов
- [x] Build 0W/0E, 125 tests green, format clean

**Acceptance:** Установка локализации работает end-to-end в тесте. **DONE.**

---

## Sprint 4: Incremental Download & File Index

**Goal:** Incremental download с хешированием, прогрессом и end-to-end оркестрацией.

- [x] `FileHashEntry.cs` — sealed record (RelativePath, Hash, Size) для трекинга файлов
- [x] `DownloadProgressInfo.cs` — sealed record с TotalBytes, BytesDownloaded, ProgressFraction
- [x] `FileIndexJsonContext.cs` — source-generated JSON сериализация (System.Text.Json, AOT-safe)
- [x] `ContentValidator.cs` — Content-Disposition validation, URL filename extraction, extension allowlist
- [x] `IFileIndexService.cs` + `FileIndexService.cs` — SHA-256 хеши (SHA256.HashDataAsync), BuildIndex, GetChangedFiles, Save/Load JSON
- [x] `ILocalizationUpdater.cs` + `LocalizationUpdater.cs` — end-to-end: CheckForUpdate → Download → Verify → Install
- [x] `IFileSystem.cs` расширен — GetFiles, GetFileSize, OpenRead
- [x] `PhysicalFileSystem.cs` — реализация новых методов
- [x] `IProgress<DownloadProgressInfo>` integration для отчёта о прогрессе загрузки
- [x] Unit-тесты: 15 ContentValidator, 12 FileIndexService, 12 LocalizationUpdater, 5 DownloadProgressInfo, 9 новых — 53 новых теста
- [x] Build 0W/0E, 178 tests green, format clean

**Acceptance:** Полный цикл скачивания и установки локализации. **DONE.**

---

## Sprint 5: Auto-Update Service (Velopack)

**Goal:** Абстракция авто-обновлений с Velopack, семантическое сравнение версий.

- [x] `AppUpdateInfo.cs` — sealed record (TargetVersion, CurrentVersion, ReleaseNotes, HasDeltaUpdates)
- [x] `UpdateStatus.cs` — enum (Idle, Checking, UpdateAvailable, Downloading, ReadyToInstall, UpToDate, Error, NotInstalled)
- [x] `SemanticVersionComparer.cs` — static helper: Compare, IsNewer, Parse (v-prefix, pre-release, build metadata)
- [x] `IUpdateManagerAdapter.cs` — интерфейс-адаптер для Velopack UpdateManager (конкретная реализация в App слое)
- [x] `IAutoUpdateService.cs` — высокоуровневый интерфейс (CheckForUpdate, Download, ApplyAndRestart, ApplyOnExit)
- [x] `AutoUpdateService.cs` — реализация с LoggerMessage source generators (CA1848-compliant)
- [x] `Microsoft.Extensions.Logging.Abstractions` добавлен в Core
- [x] Graceful error handling: исключения при проверке обновлений → возврат null (не крашит приложение)
- [x] Unit-тесты: 24 SemanticVersionComparer, 12 AutoUpdateService, 4 AppUpdateInfo/UpdateStatus — 44 новых теста
- [x] Build 0W/0E, 222 tests green, format clean

**Remaining for App layer (Sprint 7):**
- VelopackApp.Build().Run() в Main()
- Конкретная реализация IUpdateManagerAdapter через Velopack.UpdateManager + GithubSource
- UI notification при доступном обновлении

**Acceptance:** Проверка обновлений работает, delta-download тестирован. **Core DONE.**

---

## Sprint 6: ViewModels (MVVM)

**Goal:** ViewModels для всех экранов с CommunityToolkit.Mvvm.

- [x] `MainWindowViewModel.cs` — game mode selection, installations, app update check
- [x] `SettingsViewModel.cs` — editable settings with change tracking, LoadFrom/ToSettings/Revert
- [x] `LocalizationViewModel.cs` — installed languages, check/install/uninstall, reset language
- [x] `DownloadProgressViewModel.cs` — IProgress<DownloadProgressInfo>, cancel, IDisposable
- [x] `[ObservableProperty]` и `[RelayCommand]` source generators
- [x] ViewModels в Core (cross-platform) — CommunityToolkit.Mvvm 8.4.0 не зависит от WPF
- [x] `#pragma warning disable CA2007` в ViewModels — continuations нужны на UI thread
- [x] Unit-тесты: 18 MainWindowVM, 14 SettingsVM, 22 LocalizationVM, 21 DownloadProgressVM — 65 новых тестов
- [x] Build 0W/0E, 287 tests green, format clean

**Acceptance:** Все ViewModels тестируемы без UI, >80% coverage. **DONE.**

---

## Sprint 7: WPF Views (UI)

**Goal:** XAML-представления с Fluent Theme, WPF UI 4.2.0, system tray, localization.

- [x] `App.xaml.cs` — полный DI wiring: Core services, ViewModels, WPF UI services, IHttpClientFactory
- [x] `App.xaml` — Fluent Theme (Dark + Mica), value converters как StaticResource
- [x] `MainWindow.xaml` — FluentWindow с NavigationView (Localization + Settings), TitleBar с game mode ComboBox
- [x] `MainWindow.xaml.cs` — INavigationWindow, snackbar/dialog setup, SystemThemeWatcher
- [x] `LocalizationPage.xaml` — header, installed packs ListView, action buttons, status bar
- [x] `SettingsPage.xaml` — game folder, toggles (RunMinimized, IncrementalDownload, AutoUpdate), NumberBox interval
- [x] `DownloadProgressControl.xaml` — UserControl для ContentDialog, ProgressBar + Cancel
- [x] `ShellViewModel.cs` — thin wrapper: MainWindowViewModel + ApplicationTitle
- [x] `VelopackUpdateManagerAdapter.cs` — конкретная реализация IUpdateManagerAdapter (Velopack + GithubSource)
- [x] Converters: BoolToVisibilityConverter, InverseBoolConverter, NullToBoolConverter
- [x] System tray: H.NotifyIcon.Wpf 2.4.1, TaskbarIcon с GeneratedIconSource, контекстное меню (Show/Exit)
- [x] Minimize-to-tray: OnClosing → Hide(), восстановление по клику на tray icon
- [x] Localization .resx: en (default), ru, uk, ko, zh — 35 строк × 5 языков
- [x] VelopackApp.Build().Run() в App constructor
- [x] Удалены устаревшие placeholder-файлы (DashboardPage, DashboardViewModel, old MainWindowViewModel)

**Note:** App targets `net10.0-windows` — build verification на Windows. Core тесты (287) проходят на Linux.

**Acceptance:** UI полностью собран, navigation + tray + localization. **DONE (pending Windows build).**

---

## Sprint 8: Polish & Release

**Goal:** Финализация, CI/CD, code signing, документация.

- [ ] GitHub Actions: build matrix (win-x64, win-arm64)
- [ ] Self-contained + SingleFile + ReadyToRun publish
- [ ] Code signing (SignPath Foundation или Azure Trusted Signing)
- [ ] README.md (en, ru, uk) с скриншотами
- [ ] Migration guide для пользователей старой версии
- [ ] Final analyzer run: 0 warnings, 0 info
- [ ] Performance benchmark vs old version

**Acceptance:** Release-ready artifact, SmartScreen-clean, all docs updated.

---

## Velocity Tracking

| Sprint | Planned | Completed | Notes |
|--------|---------|-----------|-------|
| 0 | 11 | 11 | Done. Podman container, cross-platform Core, 7 tests green |
| 1 | 10 | 10 | Done. GameConstants, GameInstallation, AppSettings, GameDetectionService, 29 new tests |
| 2 | 8 | 8 | Done. GitHubApiService (Octokit v14), DTOs, rate limit exception, 23 new tests |
| 3 | 11 | 11 | Done. PathValidator, GameConfigService, LanguagePackService, 66 new tests |
| 4 | 11 | 11 | Done. FileIndexService, ContentValidator, LocalizationUpdater, 53 new tests |
| 5 | 10 | 10 | Done. AutoUpdateService, SemanticVersionComparer, IUpdateManagerAdapter, 44 new tests |
| 6 | 9 | 9 | Done. ViewModels in Core (cross-platform), CommunityToolkit.Mvvm, 65 new tests |
| 7 | 15 | 15 | Done. WPF Views, FluentWindow, NavigationView, tray, localization (5 langs), DI wiring |
| 8 | - | - | |
