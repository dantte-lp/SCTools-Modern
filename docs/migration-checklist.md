# Migration Checklist: Old SCTools -> SCTools-Modern

## File-by-File Migration Map

### SCToolsLib/Global/ -> SCTools.Core/Models/

| Old File | New File | Status | Notes |
|----------|----------|--------|-------|
| GameConstants.cs | Models/GameConstants.cs | [ ] | Simplify, use file-scoped namespace |
| GameFolders.cs | Models/GameInstallation.cs | [ ] | Merge into GameInstallation |
| GameInfo.cs | Models/GameInstallation.cs | [ ] | Merge into GameInstallation |
| GameMode.cs | Models/GameMode.cs | [ ] | Direct port, add docs |
| GameMutex.cs | (remove) | [ ] | Use named Mutex directly |
| MutexWrapper.cs | (remove) | [ ] | Unnecessary abstraction |

### SCToolsLib/Helpers/ -> SCTools.Core/Services/ or inline

| Old File | New File | Status | Notes |
|----------|----------|--------|-------|
| CfgReader.cs | Services/GameConfigService.cs | [ ] | Rewrite with Span<T> |
| DateTimeUtils.cs | (remove) | [ ] | Use DateTimeOffset natively |
| DisposableUtils.cs | (remove) | [ ] | Use IAsyncDisposable |
| FileUtils.cs | Helpers/FileUtils.cs | [ ] | Keep, add path validation |
| HashAlgorithmExtensions.cs | (remove) | [ ] | Use System.IO.Hashing |
| JsonHelper.cs | (remove) | [ ] | Use System.Text.Json directly |
| LocalizationAppRegistry.cs | Services/AppRegistryService.cs | [ ] | Wrap registry access |
| StreamExtensions.cs | (remove) | [ ] | Built-in async streams in .NET 10 |

### SCToolsLib/Update/ -> SCTools.Core/Services/

| Old File | New File | Status | Notes |
|----------|----------|--------|-------|
| IUpdateRepository.cs | Services/Interfaces/IGitHubApiService.cs | [ ] | Simplify interface |
| GitHubUpdateRepository.cs | Services/GitHubApiService.cs | [ ] | IHttpClientFactory, System.Text.Json |
| GiteeUpdateRepository.cs | Services/GiteeApiService.cs | [ ] | Optional: port if needed |
| ApplicationUpdater.cs | Services/AutoUpdateService.cs | [ ] | Replace with Velopack |
| FilesIndex.cs | Services/FileIndexService.cs | [ ] | SHA-256 instead of MD5 |
| UpdateInfo.cs | Models/ReleaseInfo.cs | [ ] | Simplify |
| DownloadResult.cs | Models/DownloadResult.cs | [ ] | Direct port |

### SCToolsLib/Localization/ -> SCTools.Core/Services/

| Old File | New File | Status | Notes |
|----------|----------|--------|-------|
| ILocalizationInstaller.cs | Services/Interfaces/ILanguagePackService.cs | [ ] | Merge install/uninstall |
| DefaultLocalizationInstaller.cs | Services/LanguagePackService.cs | [ ] | Fix bare catch blocks |
| GitHubLocalizationRepository.cs | (merge into GitHubApiService) | [ ] | Unified GitHub access |
| GameSettings.cs | Services/GameConfigService.cs | [ ] | Merge |
| LanguageInfo.cs | Models/LanguagePack.cs | [ ] | Rename + enhance |

### SCTools/Forms/ -> SCTools.App/Views/ + ViewModels/

| Old File | New View | New ViewModel | Status |
|----------|----------|---------------|--------|
| MainForm.cs | MainWindow.xaml | MainWindowViewModel.cs | [ ] |
| LocalizationForm.cs | LocalizationView.xaml | LocalizationViewModel.cs | [ ] |
| GameSettingsForm.cs | SettingsView.xaml | SettingsViewModel.cs | [ ] |
| ProgressForm.cs | DownloadProgressDialog.xaml | DownloadProgressViewModel.cs | [ ] |
| ManageRepositoriesForm.cs | RepositoriesView.xaml | RepositoriesViewModel.cs | [ ] |
| PromptForm.cs | (WPF MessageBox / custom dialog) | - | [ ] |

### SCTools/Repository/ -> SCTools.App/

| Old File | New File | Status | Notes |
|----------|----------|--------|-------|
| HttpNetClient.cs | (remove) | [ ] | Replaced by IHttpClientFactory |
| ConfigDataRepository.cs | Services/ConfigDataService.cs | [ ] | If SCConfigDB available |
| ProfileManager.cs | Core/Services/ProfileService.cs | [ ] | JSON-based profiles |
| RepositoryManager.cs | Core/Services/RepositoryManagerService.cs | [ ] | DI-managed |

### SCTools/Settings/ -> SCTools.Core/Models/ + appsettings.json

| Old File | New File | Status | Notes |
|----------|----------|--------|-------|
| AppSettings.cs | Models/AppSettings.cs | [ ] | IOptions<T> pattern |
| LocalizationSettings.cs | Models/LocalizationSettings.cs | [ ] | Nested in AppSettings |
| UpdateSettings.cs | Models/UpdateSettings.cs | [ ] | Nested in AppSettings |
| TimePresets.cs | (inline constants) | [ ] | Simplify |

## Security Fixes Required During Migration

- [ ] **SEC-01:** Remove SSL 3.0 -> .NET 10 uses system TLS defaults
- [ ] **SEC-02:** Process.Start() -> Uri.TryCreate() + scheme validation
- [ ] **SEC-03:** AuthToken plaintext -> Windows Credential Manager
- [ ] **SEC-04:** Path traversal -> Path.GetFullPath() + prefix check
- [ ] **SEC-05:** Content-Disposition -> validate + Path.GetFileName()
- [ ] **SEC-06:** Bare catch blocks -> typed exceptions + logging
- [ ] **SEC-07:** MD5 -> SHA-256 (System.Security.Cryptography.SHA256)

## Deleted (Not Migrated)

- All WinForms-specific code (Controls/, Adapters/, Forms/*.Designer.cs)
- NuGet config split hack (nuget.config.1 / .2)
- Legacy NLog configuration (replaced by Serilog)
- Manual HTTP client management (replaced by IHttpClientFactory)
- Custom update mechanism (replaced by Velopack)
