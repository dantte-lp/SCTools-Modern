// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Helpers;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Orchestrates localization update operations: check → download → verify → install.
/// </summary>
public sealed class LocalizationUpdater : ILocalizationUpdater
{
    private readonly IGitHubApiService _gitHubApi;
    private readonly ILanguagePackService _languagePackService;
    private readonly IFileIndexService _fileIndexService;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationUpdater"/> class.
    /// </summary>
    /// <param name="gitHubApi">GitHub API service for fetching releases and downloading assets.</param>
    /// <param name="languagePackService">Language pack service for installation.</param>
    /// <param name="fileIndexService">File index service for hash verification.</param>
    /// <param name="fileSystem">File system abstraction.</param>
    public LocalizationUpdater(
        IGitHubApiService gitHubApi,
        ILanguagePackService languagePackService,
        IFileIndexService fileIndexService,
        IFileSystem fileSystem)
    {
        _gitHubApi = gitHubApi;
        _languagePackService = languagePackService;
        _fileIndexService = fileIndexService;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public async Task<ReleaseInfo?> CheckForUpdateAsync(
        string owner,
        string repo,
        string? currentVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repo);

        var latest = await _gitHubApi.GetLatestReleaseAsync(owner, repo, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (latest is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(currentVersion))
        {
            return latest;
        }

        return string.Equals(latest.TagName, currentVersion, StringComparison.OrdinalIgnoreCase)
            ? null
            : latest;
    }

    /// <inheritdoc />
    public async Task<string> DownloadAssetAsync(
        ReleaseAssetInfo asset,
        IProgress<DownloadProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var fileName = ContentValidator.GetSafeFileNameFromUrl(asset.DownloadUrl)
                       ?? "download.tmp";

        var tempPath = Path.Combine(Path.GetTempPath(), $"sctools-{Guid.NewGuid():N}-{fileName}");

        try
        {
            var bytesProgress = progress is not null
                ? new Progress<long>(bytes => progress.Report(new DownloadProgressInfo
                {
                    TotalBytes = asset.Size > 0 ? asset.Size : null,
                    BytesDownloaded = bytes,
                    CurrentFile = asset.FileName,
                }))
                : null;

            var fileStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await using (fileStream.ConfigureAwait(false))
            {
                await _gitHubApi.DownloadAssetAsync(asset.DownloadUrl, fileStream, bytesProgress, cancellationToken)
                    .ConfigureAwait(false);
            }

            return tempPath;
        }
        catch
        {
            TryDeleteFile(tempPath);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> VerifyHashAsync(
        string filePath,
        string? expectedHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            return true;
        }

        var actualHash = await _fileIndexService.ComputeHashAsync(filePath, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<InstallStatus> UpdateAsync(
        string gameModePath,
        string languageCode,
        ReleaseAssetInfo asset,
        string? expectedHash = null,
        IProgress<DownloadProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);
        ArgumentNullException.ThrowIfNull(asset);

        var tempPath = await DownloadAssetAsync(asset, progress, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var hashValid = await VerifyHashAsync(tempPath, expectedHash, cancellationToken)
                .ConfigureAwait(false);

            if (!hashValid)
            {
                return InstallStatus.PackageError;
            }

            var result = _languagePackService.InstallFromFile(gameModePath, languageCode, tempPath);

            if (result == InstallStatus.Success)
            {
                _languagePackService.SetCurrentLanguage(gameModePath, languageCode);
            }

            return result;
        }
        finally
        {
            TryDeleteFile(tempPath);
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (_fileSystem.FileExists(path))
            {
                _fileSystem.DeleteFile(path);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup — ignore if file is locked.
        }
    }
}
