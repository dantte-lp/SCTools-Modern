# SCTools Modern — Star Citizen Companion

[![Build & Test](https://github.com/dantte-lp/SCTools-Modern/actions/workflows/build.yml/badge.svg)](https://github.com/dantte-lp/SCTools-Modern/actions/workflows/build.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![WPF UI](https://img.shields.io/badge/UI-WPF%20UI%204.2-0078d4)](https://wpfui.lepo.co/)

**[English](#english)** | **[Русский](#русский)**

---

<a id="english"></a>

## English

### About

SCTools Modern is a complete rewrite of the [original SCTools](https://github.com/h0useRus/StarCitizen) by **[h0useRus](https://github.com/h0useRus)** — a beloved companion tool used by the Star Citizen localization community for years.

The original SCTools was built on .NET Framework 4.5 with WinForms and served the community reliably for a long time. This project is a ground-up rewrite on .NET 10 with a modern architecture, while preserving the spirit and purpose of the original tool.

**Huge thanks to h0useRus for creating SCTools and for the years of work supporting the Star Citizen localization community.** This project would not exist without the foundation laid by the original.

### Features

- **Localization Management** — Install, update, and uninstall language packs for Star Citizen (LIVE, PTU, EPTU)
- **Incremental Downloads** — Only downloads changed files, saving bandwidth
- **Auto-Updates** — Delta updates via [Velopack](https://velopack.io/) with automatic background checks
- **Multi-Language UI** — English, Russian, Ukrainian, Korean, Chinese (Simplified)
- **System Tray** — Minimize to tray, background update checks
- **Fluent Design** — Windows 11 native look with Mica backdrop and dark theme
- **Game Mode Detection** — Automatically detects LIVE, PTU, and EPTU installations

### Installation

Download the latest release from [GitHub Releases](https://github.com/dantte-lp/SCTools-Modern/releases).

| Platform | Download |
|----------|----------|
| Windows x64 | `SCTools-win-x64.zip` |
| Windows ARM64 | `SCTools-win-arm64.zip` |

Self-contained — no .NET runtime installation required. After first install, updates are delivered automatically via Velopack delta packages.

### Building from Source

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), Windows 10/11

```bash
dotnet build -c Release          # Full solution (Windows)
dotnet test -c Release           # Run 287 tests
dotnet publish src/SCTools.App/ -c Release -r win-x64 --self-contained
```

**Container development** (Core library + tests on Linux):

```bash
podman build -t sctools-dev -f Containerfile .
podman run --rm -v ./:/workspace:Z sctools-dev dotnet test tests/SCTools.Tests/ -c Release
```

### Architecture

```
SCTools.sln
  src/
    SCTools.App/       WPF application (net10.0-windows)
    SCTools.Core/      Business logic (net10.0, cross-platform)
  tests/
    SCTools.Tests/     287 xUnit v3 tests
```

Core library is fully cross-platform (builds and tests on Linux). WPF UI is Windows-only.
ViewModels live in Core (CommunityToolkit.Mvvm) for full testability without Windows.

### Tech Stack

| Component | Library |
|-----------|---------|
| Runtime | .NET 10, C# 14 |
| UI | WPF UI 4.2.0 (Fluent Design) |
| MVVM | CommunityToolkit.Mvvm 8.4.0 |
| GitHub API | Octokit 14.0.0 |
| Auto-Update | Velopack 0.0.1298 |
| System Tray | H.NotifyIcon.Wpf 2.4.1 |
| Logging | Serilog 10.0.0 |
| Tests | xUnit v3 + NSubstitute + FluentAssertions |

### Contributing

1. Fork the repository
2. Create a feature branch
3. Ensure zero warnings: `dotnet build -c Release`
4. Ensure all tests pass: `dotnet test -c Release`
5. Submit a pull request

### Security

See [SECURITY.md](SECURITY.md) for vulnerability reporting policy.

### License

MIT — see [LICENSE](LICENSE).

---

<a id="русский"></a>

## Русский

### О проекте

SCTools Modern — это полная переработка [оригинального SCTools](https://github.com/h0useRus/StarCitizen) авторства **[h0useRus](https://github.com/h0useRus)** — инструмента-компаньона, которым сообщество локализации Star Citizen пользовалось на протяжении многих лет.

Оригинальный SCTools был построен на .NET Framework 4.5 с WinForms и долго верно служил сообществу. Этот проект — переписан с нуля на .NET 10 с современной архитектурой, но с сохранением духа и назначения оригинального инструмента.

**Огромная благодарность h0useRus за создание SCTools и годы работы в поддержку сообщества локализации Star Citizen.** Этот проект не появился бы без фундамента, заложенного оригиналом.

### Возможности

- **Управление локализацией** — установка, обновление и удаление языковых пакетов (LIVE, PTU, EPTU)
- **Инкрементальная загрузка** — загружаются только изменённые файлы, экономия трафика
- **Авто-обновления** — дельта-обновления через [Velopack](https://velopack.io/)
- **Мультиязычный интерфейс** — английский, русский, украинский, корейский, китайский
- **Системный трей** — сворачивание в трей, фоновая проверка обновлений
- **Fluent Design** — нативный вид Windows 11 с Mica и тёмной темой
- **Обнаружение режимов игры** — автоматическое определение LIVE, PTU, EPTU

### Установка

Скачайте последнюю версию из [GitHub Releases](https://github.com/dantte-lp/SCTools-Modern/releases).

| Платформа | Файл |
|-----------|------|
| Windows x64 | `SCTools-win-x64.zip` |
| Windows ARM64 | `SCTools-win-arm64.zip` |

Приложение самодостаточное — установка .NET не требуется. После первого запуска обновления доставляются автоматически.

### Сборка из исходников

**Требования:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), Windows 10/11

```bash
dotnet build -c Release          # Полное решение (Windows)
dotnet test -c Release           # 287 тестов
dotnet publish src/SCTools.App/ -c Release -r win-x64 --self-contained
```

**Контейнерная разработка** (Core + тесты на Linux):

```bash
podman build -t sctools-dev -f Containerfile .
podman run --rm -v ./:/workspace:Z sctools-dev dotnet test tests/SCTools.Tests/ -c Release
```

### Архитектура

Core библиотека полностью кросс-платформенная (сборка и тесты на Linux). WPF UI — только Windows.
ViewModels живут в Core (CommunityToolkit.Mvvm) для полной тестируемости без Windows.

### Участие в разработке

1. Форкните репозиторий
2. Создайте feature-ветку
3. Убедитесь: 0 warnings при `dotnet build -c Release`
4. Убедитесь: все тесты проходят
5. Откройте pull request

### Безопасность

Смотрите [SECURITY.md](SECURITY.md).

### Лицензия

MIT — см. [LICENSE](LICENSE).

---

### Благодарности / Acknowledgments

- **[h0useRus](https://github.com/h0useRus)** — автор оригинального [SCTools](https://github.com/h0useRus/StarCitizen), заложивший фундамент этого проекта
- Сообщество локализации Star Citizen — за обратную связь и тестирование
- [WPF UI](https://wpfui.lepo.co/) — Fluent Design для WPF
- [Velopack](https://velopack.io/) — фреймворк авто-обновлений
