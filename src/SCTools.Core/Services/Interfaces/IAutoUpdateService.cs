// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Manages automatic application updates.
/// Abstracts the underlying update framework (Velopack) for testability.
/// </summary>
public interface IAutoUpdateService
{
    /// <summary>
    /// Gets a value indicating whether the application is installed
    /// (as opposed to running from IDE or portable mode).
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    /// Gets the currently installed application version, or <c>null</c> if not installed.
    /// </summary>
    string? CurrentVersion { get; }

    /// <summary>
    /// Checks whether an update is available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update info if available, or <c>null</c> if up-to-date.</returns>
    Task<AppUpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the available update.
    /// </summary>
    /// <param name="updateInfo">The update to download (from <see cref="CheckForUpdateAsync"/>).</param>
    /// <param name="progress">Optional progress reporter (0-100 percent).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the download operation.</returns>
    Task DownloadUpdateAsync(
        AppUpdateInfo updateInfo,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a downloaded update and restarts the application.
    /// This method does not return — it exits the process.
    /// </summary>
    /// <param name="restartArgs">Optional command-line arguments for the restarted process.</param>
    void ApplyUpdateAndRestart(string[]? restartArgs = null);

    /// <summary>
    /// Applies a downloaded update when the application exits.
    /// </summary>
    void ApplyUpdateOnExit();
}
