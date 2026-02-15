// Licensed to the SCTools project under the MIT license.

// ViewModels must not use ConfigureAwait(false) — continuations need the UI synchronization context.
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.ViewModels;

/// <summary>
/// ViewModel for the localization management page.
/// Manages installed language packs, updates, and language switching.
/// </summary>
public sealed partial class LocalizationViewModel : ObservableObject
{
    private readonly ILanguagePackService _languagePack;
    private readonly ILocalizationUpdater _updater;

    /// <summary>Gets or sets the path to the active game mode directory.</summary>
    [ObservableProperty]
    private string? _gameModePath;

    /// <summary>Gets or sets the currently active language code.</summary>
    [ObservableProperty]
    private string? _currentLanguage;

    /// <summary>Gets or sets the selected language pack in the list.</summary>
    [ObservableProperty]
    private LanguagePack? _selectedLanguage;

    /// <summary>Gets or sets the latest available release, if newer than installed.</summary>
    [ObservableProperty]
    private ReleaseInfo? _availableUpdate;

    /// <summary>Gets or sets a value indicating whether an update check is in progress.</summary>
    [ObservableProperty]
    private bool _isChecking;

    /// <summary>Gets or sets a value indicating whether an install/uninstall is in progress.</summary>
    [ObservableProperty]
    private bool _isInstalling;

    /// <summary>Gets or sets the status message displayed to the user.</summary>
    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationViewModel"/> class.
    /// </summary>
    /// <param name="languagePack">Language pack management service.</param>
    /// <param name="updater">Localization update orchestrator.</param>
    public LocalizationViewModel(ILanguagePackService languagePack, ILocalizationUpdater updater)
    {
        _languagePack = languagePack;
        _updater = updater;
    }

    /// <summary>Gets the installed language packs.</summary>
    public ObservableCollection<LanguagePack> InstalledLanguages { get; } = [];

    /// <summary>Gets or sets the GitHub repository owner for localization releases.</summary>
    public string RepositoryOwner { get; set; } = "h0useRus";

    /// <summary>Gets or sets the GitHub repository name for localization releases.</summary>
    public string RepositoryName { get; set; } = "StarCitizen";

    partial void OnGameModePathChanged(string? value)
    {
        RefreshInstalledLanguages();
    }

    /// <summary>
    /// Refreshes the list of installed language packs for the current game mode path.
    /// </summary>
    [RelayCommand]
    private void RefreshInstalledLanguages()
    {
        InstalledLanguages.Clear();
        AvailableUpdate = null;
        CurrentLanguage = null;
        SelectedLanguage = null;

        if (string.IsNullOrEmpty(GameModePath))
        {
            return;
        }

        var languages = _languagePack.GetInstalledLanguages(GameModePath);

        foreach (var lang in languages)
        {
            InstalledLanguages.Add(lang);
        }

        CurrentLanguage = _languagePack.GetCurrentLanguage(GameModePath);
        SelectedLanguage = InstalledLanguages.Count > 0 ? InstalledLanguages[0] : null;
    }

    /// <summary>
    /// Checks for a newer localization release on GitHub.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    [RelayCommand]
    private async Task CheckForUpdateAsync(CancellationToken cancellationToken)
    {
        IsChecking = true;
        StatusMessage = "Checking for updates...";

        try
        {
            string? currentVersion = SelectedLanguage?.Version;

            AvailableUpdate = await _updater.CheckForUpdateAsync(
                RepositoryOwner,
                RepositoryName,
                currentVersion,
                cancellationToken);

            StatusMessage = AvailableUpdate is not null
                ? $"Update available: {AvailableUpdate.TagName}"
                : "Localization is up to date.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Update check cancelled.";
            throw;
        }
        finally
        {
            IsChecking = false;
        }
    }

    /// <summary>
    /// Downloads and installs the available localization update.
    /// </summary>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    [RelayCommand]
    private async Task InstallUpdateAsync(
        IProgress<DownloadProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        if (AvailableUpdate is null || string.IsNullOrEmpty(GameModePath))
        {
            return;
        }

        var assets = AvailableUpdate.Assets;
        var asset = assets.Count > 0 ? assets[0] : null;

        if (asset is null)
        {
            StatusMessage = "No downloadable asset found.";
            return;
        }

        IsInstalling = true;
        StatusMessage = "Installing update...";

        try
        {
            var result = await _updater.UpdateAsync(
                GameModePath,
                CurrentLanguage ?? GameConstants.DefaultLanguage,
                asset,
                expectedHash: null,
                progress,
                cancellationToken);

            StatusMessage = result == InstallStatus.Success
                ? "Update installed successfully."
                : $"Installation failed: {result}";

            if (result == InstallStatus.Success)
            {
                RefreshInstalledLanguages();
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Installation cancelled.";
            throw;
        }
        finally
        {
            IsInstalling = false;
        }
    }

    /// <summary>
    /// Uninstalls the selected language pack.
    /// </summary>
    [RelayCommand]
    private void Uninstall()
    {
        if (SelectedLanguage is null || string.IsNullOrEmpty(GameModePath))
        {
            return;
        }

        var removed = _languagePack.Uninstall(GameModePath, SelectedLanguage.LanguageCode);

        StatusMessage = removed
            ? $"Language '{SelectedLanguage.LanguageCode}' uninstalled."
            : $"Language '{SelectedLanguage.LanguageCode}' not found.";

        RefreshInstalledLanguages();
    }

    /// <summary>
    /// Resets the language to English (removes g_language from user.cfg).
    /// </summary>
    [RelayCommand]
    private void ResetLanguage()
    {
        if (string.IsNullOrEmpty(GameModePath))
        {
            return;
        }

        _languagePack.ResetLanguage(GameModePath);
        CurrentLanguage = null;
        StatusMessage = "Language reset to English.";
    }
}
