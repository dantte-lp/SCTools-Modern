// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Provides access to the GitHub API for fetching releases and downloading assets.
/// </summary>
public interface IGitHubApiService
{
    /// <summary>
    /// Gets all releases for the configured repository.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="includePreReleases">Whether to include pre-releases.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of releases, newest first.</returns>
    Task<IReadOnlyList<ReleaseInfo>> GetReleasesAsync(
        string owner,
        string repo,
        bool includePreReleases = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest release for the configured repository.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="includePreReleases">Whether to consider pre-releases as latest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest release, or <c>null</c> if none found.</returns>
    Task<ReleaseInfo?> GetLatestReleaseAsync(
        string owner,
        string repo,
        bool includePreReleases = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a release asset to the specified stream.
    /// </summary>
    /// <param name="downloadUrl">The browser download URL of the asset.</param>
    /// <param name="destination">The stream to write the asset content to.</param>
    /// <param name="progress">Optional progress reporter (bytes downloaded).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the download operation.</returns>
    Task DownloadAssetAsync(
        Uri downloadUrl,
        Stream destination,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current GitHub API rate limit status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rate limit information.</returns>
    Task<GitHubRateLimitInfo> GetRateLimitAsync(CancellationToken cancellationToken = default);
}
