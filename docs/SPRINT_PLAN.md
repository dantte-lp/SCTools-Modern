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

**Goal:** Скачивание и установка локализаций.

- [ ] `ILanguagePackService.cs` — интерфейс
- [ ] `LanguagePackService.cs` — скачивание, верификация, установка
- [ ] `LanguagePack.cs` — модель языкового пакета
- [ ] Incremental download (FilesIndex с SHA-256 вместо MD5)
- [ ] Path traversal protection (Path.GetFullPath + prefix check)
- [ ] Content-Disposition validation
- [ ] `IProgress<T>` для отчёта о прогрессе
- [ ] Unit-тесты для всех edge cases

**Acceptance:** Установка локализации работает end-to-end в тесте.

---

## Sprint 4: Game Config Service

**Goal:** Чтение/запись user.cfg и профили настроек.

- [ ] `IGameConfigService.cs` — интерфейс
- [ ] `GameConfigService.cs` — парсинг и запись user.cfg
- [ ] `ProfileManager.cs` — сохранение/загрузка профилей настроек
- [ ] Integration с SCConfigDB (если пакет портирован на .NET 10)
- [ ] Unit-тесты парсинга cfg

**Acceptance:** Настройки читаются, пишутся, профили сохраняются.

---

## Sprint 5: Auto-Update Service (Velopack)

**Goal:** Замена текущего update-механизма на Velopack.

- [ ] `IAutoUpdateService.cs` — интерфейс
- [ ] `AutoUpdateService.cs` — Velopack integration
- [ ] `VelopackApp.Build().Run()` в Main()
- [ ] GitHub Releases как источник обновлений
- [ ] Delta updates (скачивание только изменений)
- [ ] UI notification при доступном обновлении
- [ ] Unit-тесты (mock update manager)

**Acceptance:** Проверка обновлений работает, delta-download тестирован.

---

## Sprint 6: ViewModels (MVVM)

**Goal:** ViewModels для всех экранов с CommunityToolkit.Mvvm.

- [ ] `MainWindowViewModel.cs` — game mode selection, status
- [ ] `SettingsViewModel.cs` — настройки приложения
- [ ] `LocalizationViewModel.cs` — управление локализациями
- [ ] `DownloadProgressViewModel.cs` — прогресс скачивания
- [ ] `[ObservableProperty]` и `[RelayCommand]` source generators
- [ ] Unit-тесты для каждого ViewModel (команды, состояния, ошибки)

**Acceptance:** Все ViewModels тестируемы без UI, >80% coverage.

---

## Sprint 7: WPF Views (UI)

**Goal:** XAML-представления с Fluent Theme.

- [ ] `MainWindow.xaml` — основное окно с TabControl
- [ ] `SettingsView.xaml` — настройки
- [ ] `LocalizationView.xaml` — управление языками
- [ ] `DownloadProgressDialog.xaml` — диалог прогресса
- [ ] Fluent Theme integration (`<Application.Resources>`)
- [ ] System tray via H.NotifyIcon.Wpf
- [ ] Localization (en, ru, uk, ko, zh) через .resx

**Acceptance:** UI работает, переключение тем, трей-иконка.

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
| 2 | - | - | |
| 3 | - | - | |
| 4 | - | - | |
| 5 | - | - | |
| 6 | - | - | |
| 7 | - | - | |
| 8 | - | - | |
