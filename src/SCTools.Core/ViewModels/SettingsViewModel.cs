// Licensed to the SCTools project under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SCTools.Core.Models;

namespace SCTools.Core.ViewModels;

/// <summary>
/// ViewModel for the settings page.
/// Exposes editable copies of <see cref="AppSettings"/> with change tracking.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private AppSettings _original = new();

    /// <summary>Gets or sets the root StarCitizen game folder path.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private string? _gameFolder;

    /// <summary>Gets or sets a value indicating whether the app starts minimized to tray.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private bool _runMinimized;

    /// <summary>Gets or sets a value indicating whether incremental downloads are enabled.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private bool _allowIncrementalDownload = true;

    /// <summary>Gets or sets a value indicating whether automatic update checks are enabled.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private bool _regularCheckForUpdates = true;

    /// <summary>Gets or sets the interval between automatic update checks, in minutes.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    private int _updateCheckIntervalMinutes = 60;

    /// <summary>
    /// Gets a value indicating whether any setting has been modified since last load/save.
    /// </summary>
    public bool HasChanges =>
        GameFolder != _original.GameFolder ||
        RunMinimized != _original.RunMinimized ||
        AllowIncrementalDownload != _original.AllowIncrementalDownload ||
        RegularCheckForUpdates != _original.RegularCheckForUpdates ||
        UpdateCheckIntervalMinutes != _original.UpdateCheckIntervalMinutes;

    /// <summary>
    /// Loads settings values from an <see cref="AppSettings"/> instance.
    /// Resets the <see cref="HasChanges"/> flag.
    /// </summary>
    /// <param name="settings">The settings to load from.</param>
    public void LoadFrom(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _original = new AppSettings
        {
            GameFolder = settings.GameFolder,
            RunMinimized = settings.RunMinimized,
            AllowIncrementalDownload = settings.AllowIncrementalDownload,
            RegularCheckForUpdates = settings.RegularCheckForUpdates,
            UpdateCheckIntervalMinutes = settings.UpdateCheckIntervalMinutes,
        };

        GameFolder = settings.GameFolder;
        RunMinimized = settings.RunMinimized;
        AllowIncrementalDownload = settings.AllowIncrementalDownload;
        RegularCheckForUpdates = settings.RegularCheckForUpdates;
        UpdateCheckIntervalMinutes = settings.UpdateCheckIntervalMinutes;
    }

    /// <summary>
    /// Returns a new <see cref="AppSettings"/> instance with the current ViewModel values.
    /// </summary>
    /// <returns>A settings object reflecting the current state.</returns>
    public AppSettings ToSettings() => new()
    {
        GameFolder = GameFolder,
        RunMinimized = RunMinimized,
        AllowIncrementalDownload = AllowIncrementalDownload,
        RegularCheckForUpdates = RegularCheckForUpdates,
        UpdateCheckIntervalMinutes = UpdateCheckIntervalMinutes,
    };

    /// <summary>
    /// Reverts all settings to the last loaded values.
    /// </summary>
    [RelayCommand]
    private void Revert()
    {
        LoadFrom(_original);
    }
}
