// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Adapter interface for the underlying update framework (e.g. Velopack UpdateManager).
/// Allows the <see cref="AutoUpdateService"/> to remain cross-platform and testable
/// while the concrete implementation lives in the App layer.
/// </summary>
public interface IUpdateManagerAdapter
{
    /// <summary>
    /// Gets a value indicating whether the application is installed.
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    /// Gets the currently installed version string, or <c>null</c> if not installed.
    /// </summary>
    string? CurrentVersion { get; }

    /// <summary>
    /// Checks for available updates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update info if available, or <c>null</c> if up-to-date.</returns>
    Task<AppUpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the specified update.
    /// </summary>
    /// <param name="updateInfo">The update to download.</param>
    /// <param name="progress">Optional progress reporter (0-100 percent).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the download operation.</returns>
    Task DownloadUpdateAsync(
        AppUpdateInfo updateInfo,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies the downloaded update and restarts the application.
    /// </summary>
    /// <param name="restartArgs">Optional command-line arguments for restart.</param>
    void ApplyUpdateAndRestart(string[]? restartArgs = null);

    /// <summary>
    /// Schedules the update to be applied when the application exits.
    /// </summary>
    void ApplyUpdateOnExit();
}
