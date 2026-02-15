// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Orchestrates the end-to-end localization update flow:
/// check for updates → download → verify → install.
/// </summary>
public interface ILocalizationUpdater
{
    /// <summary>
    /// Checks whether a newer localization version is available.
    /// </summary>
    /// <param name="owner">GitHub repository owner.</param>
    /// <param name="repo">GitHub repository name.</param>
    /// <param name="currentVersion">The currently installed version tag, or <c>null</c> if not installed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest release if newer than current, or <c>null</c> if up-to-date.</returns>
    Task<ReleaseInfo?> CheckForUpdateAsync(
        string owner,
        string repo,
        string? currentVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a release asset to a temporary file and returns the file path.
    /// </summary>
    /// <param name="asset">The asset to download.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path to the downloaded temporary file.</returns>
    Task<string> DownloadAssetAsync(
        ReleaseAssetInfo asset,
        IProgress<DownloadProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the SHA-256 hash of a downloaded file.
    /// </summary>
    /// <param name="filePath">Path to the file to verify.</param>
    /// <param name="expectedHash">Expected SHA-256 hash (hex string), or <c>null</c> to skip verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the hash matches or if no expected hash was provided; otherwise <c>false</c>.</returns>
    Task<bool> VerifyHashAsync(
        string filePath,
        string? expectedHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a full update: download, verify, and install a localization pack.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="languageCode">Target language code.</param>
    /// <param name="asset">The release asset to download and install.</param>
    /// <param name="expectedHash">Expected SHA-256 hash, or <c>null</c> to skip verification.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The installation status.</returns>
    Task<InstallStatus> UpdateAsync(
        string gameModePath,
        string languageCode,
        ReleaseAssetInfo asset,
        string? expectedHash = null,
        IProgress<DownloadProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);
}
