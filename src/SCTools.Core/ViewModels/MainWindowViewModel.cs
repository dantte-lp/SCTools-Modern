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
/// ViewModel for the main application window.
/// Manages game mode selection, installation detection, and app update status.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IGameDetectionService _gameDetection;
    private readonly IAutoUpdateService _autoUpdate;

    /// <summary>Gets or sets the currently selected game mode.</summary>
    [ObservableProperty]
    private GameMode _selectedGameMode = GameMode.Live;

    /// <summary>Gets or sets the root StarCitizen game folder path.</summary>
    [ObservableProperty]
    private string? _gameFolder;

    /// <summary>Gets or sets the installation matching the selected game mode.</summary>
    [ObservableProperty]
    private GameInstallation? _selectedInstallation;

    /// <summary>Gets or sets the status bar message.</summary>
    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>Gets or sets a value indicating whether a background operation is in progress.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Gets or sets the current application version string.</summary>
    [ObservableProperty]
    private string? _appVersion;

    /// <summary>Gets or sets the available application update, if any.</summary>
    [ObservableProperty]
    private AppUpdateInfo? _availableUpdate;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="gameDetection">Game installation detection service.</param>
    /// <param name="autoUpdate">Application auto-update service.</param>
    public MainWindowViewModel(IGameDetectionService gameDetection, IAutoUpdateService autoUpdate)
    {
        _gameDetection = gameDetection;
        _autoUpdate = autoUpdate;
        AppVersion = _autoUpdate.CurrentVersion;
    }

    /// <summary>Gets the available game modes.</summary>
    public IReadOnlyList<GameMode> AvailableGameModes { get; } = Enum.GetValues<GameMode>();

    /// <summary>Gets the detected game installations.</summary>
    public ObservableCollection<GameInstallation> Installations { get; } = [];

    /// <summary>
    /// Gets the full path to the selected game mode directory, or <c>null</c> if no installation is selected.
    /// </summary>
    public string? SelectedGameModePath => SelectedInstallation?.RootFolderPath;

    /// <summary>
    /// Loads initial state from application settings.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    public void Initialize(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        GameFolder = settings.GameFolder;

        if (!string.IsNullOrEmpty(GameFolder))
        {
            DetectInstallations();
        }
    }

    partial void OnSelectedGameModeChanged(GameMode value)
    {
        SelectedInstallation = Installations.FirstOrDefault(i => i.Mode == value);
        OnPropertyChanged(nameof(SelectedGameModePath));
    }

    partial void OnSelectedInstallationChanged(GameInstallation? value)
    {
        OnPropertyChanged(nameof(SelectedGameModePath));
    }

    /// <summary>
    /// Detects game installations in the current game folder.
    /// </summary>
    [RelayCommand]
    private void DetectInstallations()
    {
        Installations.Clear();
        SelectedInstallation = null;

        if (string.IsNullOrEmpty(GameFolder))
        {
            StatusMessage = "Game folder not set.";
            return;
        }

        var found = _gameDetection.DetectInstallations(GameFolder);

        foreach (var installation in found)
        {
            Installations.Add(installation);
        }

        SelectedInstallation = Installations.FirstOrDefault(i => i.Mode == SelectedGameMode)
            ?? Installations.FirstOrDefault();

        StatusMessage = Installations.Count > 0
            ? $"Found {Installations.Count} installation(s)."
            : "No installations detected.";
    }

    /// <summary>
    /// Checks for available application updates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    [RelayCommand]
    private async Task CheckForAppUpdateAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        StatusMessage = "Checking for updates...";

        try
        {
            AvailableUpdate = await _autoUpdate.CheckForUpdateAsync(cancellationToken);

            StatusMessage = AvailableUpdate is not null
                ? $"Update available: v{AvailableUpdate.TargetVersion}"
                : "Application is up to date.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Update check cancelled.";
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
