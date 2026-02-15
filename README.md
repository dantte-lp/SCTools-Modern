# SCTools — Star Citizen Companion

[![Build & Test](https://github.com/h0useRus/StarCitizen/actions/workflows/build.yml/badge.svg)](https://github.com/h0useRus/StarCitizen/actions/workflows/build.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![WPF UI](https://img.shields.io/badge/UI-WPF%20UI%204.2-0078d4)](https://wpfui.lepo.co/)

Desktop companion tool for **Star Citizen** — manage localization packs, game settings, and auto-updates.

Built with .NET 10, WPF UI (Fluent Design), and modern C# 14.

---

## Features

- **Localization Management** — Install, update, and uninstall language packs for Star Citizen (LIVE, PTU, EPTU)
- **Incremental Downloads** — Only downloads changed files, saving bandwidth
- **Auto-Updates** — Delta updates via [Velopack](https://velopack.io/) (automatic background checks)
- **Multi-Language UI** — English, Russian, Ukrainian, Korean, Chinese (Simplified)
- **System Tray** — Minimize to tray, background update checks
- **Fluent Design** — WPF UI with Mica backdrop and dark theme
- **Game Mode Detection** — Automatically detects LIVE, PTU, and EPTU installations

## Screenshots

<!-- TODO: Add screenshots when UI is finalized -->

## Installation

### Download

Download the latest release from [GitHub Releases](https://github.com/h0useRus/StarCitizen/releases).

| Platform | Download |
|----------|----------|
| Windows x64 | `SCTools-win-x64.zip` |
| Windows ARM64 | `SCTools-win-arm64.zip` |

The application is self-contained — no .NET runtime installation required.

### Auto-Update

After installation, SCTools updates itself automatically via Velopack delta updates.

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (preview)
- Windows 10/11 (WPF requires Windows)
- Optional: [Podman](https://podman.io/) for containerized Core development on Linux

### Build

```bash
# Full solution (Windows only)
dotnet build -c Release

# Core library only (cross-platform)
dotnet build src/SCTools.Core/ -c Release

# Run tests
dotnet test -c Release

# Publish self-contained executable
dotnet publish src/SCTools.App/ -c Release -r win-x64 --self-contained
```

### Container Development

Core library and tests can be built and run inside a Podman container (Debian trixie, .NET 10 SDK):

```bash
# Build dev container
podman build -t sctools-dev -f Containerfile .

# Build Core
podman run --rm -v ./:/workspace:Z sctools-dev dotnet build src/SCTools.Core/ -c Release

# Run tests (287 tests)
podman run --rm -v ./:/workspace:Z sctools-dev dotnet test tests/SCTools.Tests/ -c Release

# Format check
podman run --rm -v ./:/workspace:Z sctools-dev dotnet format src/SCTools.Core/ --verify-no-changes --severity warn
```

## Architecture

```
SCTools.sln
  src/
    SCTools.App/       WPF application (net10.0-windows)
      Views/             XAML pages + controls
      Converters/        Value converters
      Services/          WPF-specific services (VelopackAdapter)
      Resources/         Localization .resx files
    SCTools.Core/      Business logic library (net10.0, cross-platform)
      Models/            Domain models (GameMode, LanguagePack, AppSettings)
      Services/          Core services (GitHub API, language packs, config, auto-update)
      ViewModels/        MVVM ViewModels (CommunityToolkit.Mvvm)
      Helpers/           Utilities (PathValidator, ContentValidator, FileIndex)
  tests/
    SCTools.Tests/     xUnit v3 tests (287 tests)
```

### Key Design Decisions

- **ViewModels in Core** — CommunityToolkit.Mvvm is cross-platform; ViewModels live in Core for Linux testability
- **IUpdateManagerAdapter** — Velopack abstraction keeps Core free of Windows dependencies
- **IFileSystem** — All file operations go through an interface for testability
- **ConfigureAwait** — Services always use `ConfigureAwait(false)`; ViewModels never do (UI thread)
- **Zero warnings** — `TreatWarningsAsErrors`, StyleCop, .NET analyzers at max level

### Tech Stack

| Component | Library | Version |
|-----------|---------|---------|
| Runtime | .NET | 10.0 |
| UI Framework | WPF UI | 4.2.0 |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 |
| DI/Config | Microsoft.Extensions.Hosting | 10.0.2 |
| Logging | Serilog | 10.0.0 |
| GitHub API | Octokit | 14.0.0 |
| Auto-Update | Velopack | 0.0.1298 |
| System Tray | H.NotifyIcon.Wpf | 2.4.1 |
| Archives | SharpCompress | 0.41.0 |
| Hashing | System.IO.Hashing (xxHash3) | 10.0.2 |
| Tests | xUnit v3 + NSubstitute + FluentAssertions | 3.2.2 |
| Analyzers | StyleCop + .NET Analyzers | latest |

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Write tests for your changes
4. Ensure `dotnet build -c Release` has zero warnings
5. Ensure `dotnet test -c Release` passes all tests
6. Ensure `dotnet format --verify-no-changes --severity warn` is clean
7. Submit a pull request

## Security

See [SECURITY.md](SECURITY.md) for vulnerability reporting policy.

- Tokens stored in Windows Credential Manager (never plaintext)
- TLS uses system defaults (no hardcoded versions)
- All file paths validated against traversal attacks
- URLs validated before external process launch
- NuGet audit enabled with NU1901-NU1904 as errors

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Original [SCTools](https://github.com/h0useRus/StarCitizen) by h0useRus
- [WPF UI](https://wpfui.lepo.co/) by lepo.co
- [Velopack](https://velopack.io/) for delta updates
- Star Citizen localization community
