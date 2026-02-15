// Licensed to the SCTools project under the MIT license.

using System.Net;
using Octokit;
using SCTools.Core.Exceptions;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Provides access to the GitHub API using Octokit for metadata
/// and <see cref="HttpClient"/> for asset downloads.
/// </summary>
public sealed class GitHubApiService : IGitHubApiService
{
    /// <summary>
    /// Buffer size for streaming asset downloads (16 KB).
    /// </summary>
    internal const int DownloadBufferSize = 16 * 1024;

    private readonly IGitHubClient _gitHubClient;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubApiService"/> class.
    /// </summary>
    /// <param name="gitHubClient">Octokit GitHub client for API metadata.</param>
    /// <param name="httpClient">HTTP client for streaming asset downloads.</param>
    public GitHubApiService(IGitHubClient gitHubClient, HttpClient httpClient)
    {
        _gitHubClient = gitHubClient;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReleaseInfo>> GetReleasesAsync(
        string owner,
        string repo,
        bool includePreReleases = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repo);

        try
        {
            var releases = await _gitHubClient.Repository.Release
                .GetAll(owner, repo)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            return releases
                .Where(r => !r.Draft && (includePreReleases || !r.Prerelease))
                .Select(MapRelease)
                .ToList();
        }
        catch (RateLimitExceededException ex)
        {
            throw CreateRateLimitException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<ReleaseInfo?> GetLatestReleaseAsync(
        string owner,
        string repo,
        bool includePreReleases = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repo);

        if (includePreReleases)
        {
            var releases = await GetReleasesAsync(owner, repo, includePreReleases: true, cancellationToken)
                .ConfigureAwait(false);
            return releases.Count > 0 ? releases[0] : null;
        }

        try
        {
            var release = await _gitHubClient.Repository.Release
                .GetLatest(owner, repo)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            return MapRelease(release);
        }
        catch (NotFoundException)
        {
            return null;
        }
        catch (RateLimitExceededException ex)
        {
            throw CreateRateLimitException(ex);
        }
    }

    /// <inheritdoc />
    public async Task DownloadAssetAsync(
        Uri downloadUrl,
        Stream destination,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(downloadUrl);
        ArgumentNullException.ThrowIfNull(destination);

        using var response = await _httpClient
            .GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
        {
            throw await CreateRateLimitExceptionFromResponseAsync(response, cancellationToken)
                .ConfigureAwait(false);
        }

        response.EnsureSuccessStatusCode();

        var contentStream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        await using (contentStream.ConfigureAwait(false))
        {
            if (progress is null)
            {
                await contentStream.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
                return;
            }

            var buffer = new byte[DownloadBufferSize];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress.Report(totalBytesRead);
            }
        }
    }

    /// <inheritdoc />
    public async Task<GitHubRateLimitInfo> GetRateLimitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimit = await _gitHubClient.RateLimit
                .GetRateLimits()
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            var core = rateLimit.Resources.Core;
            return new GitHubRateLimitInfo
            {
                Limit = core.Limit,
                Remaining = core.Remaining,
                ResetTime = core.Reset,
            };
        }
        catch (RateLimitExceededException ex)
        {
            throw CreateRateLimitException(ex);
        }
    }

    private static ReleaseInfo MapRelease(Release release)
    {
        return new ReleaseInfo
        {
            TagName = release.TagName,
            Name = release.Name,
            Body = release.Body,
            PublishedAt = release.PublishedAt,
            IsPreRelease = release.Prerelease,
            IsDraft = release.Draft,
            HtmlUrl = string.IsNullOrEmpty(release.HtmlUrl) ? null : new Uri(release.HtmlUrl),
            Assets = release.Assets
                .Select(a => new ReleaseAssetInfo
                {
                    FileName = a.Name,
                    Size = a.Size,
                    DownloadUrl = new Uri(a.BrowserDownloadUrl),
                    ContentType = a.ContentType,
                })
                .ToList(),
        };
    }

    private static GitHubRateLimitException CreateRateLimitException(RateLimitExceededException ex)
    {
        return new GitHubRateLimitException(
            ex.Reset,
            ex.Limit,
            remaining: 0)
        {
            Source = nameof(GitHubApiService),
        };
    }

    private static async Task<GitHubRateLimitException> CreateRateLimitExceptionFromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var resetTime = DateTimeOffset.UtcNow.AddMinutes(1);
        var limit = 0;

        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
        {
            var resetStr = resetValues.FirstOrDefault();
            if (long.TryParse(resetStr, out var unixSeconds))
            {
                resetTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }
        }

        if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limitValues))
        {
            var limitStr = limitValues.FirstOrDefault();
            _ = int.TryParse(limitStr, out limit);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return new GitHubRateLimitException(
            resetTime,
            limit,
            remaining: 0)
        {
            Source = $"HTTP {(int)response.StatusCode}: {body}",
        };
    }
}
