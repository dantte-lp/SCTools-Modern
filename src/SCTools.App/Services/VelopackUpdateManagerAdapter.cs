// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;
using Velopack;
using Velopack.Sources;

namespace SCTools.App.Services;

/// <summary>
/// Concrete implementation of <see cref="IUpdateManagerAdapter"/> using Velopack.
/// This adapter bridges the Core abstraction with the actual Velopack UpdateManager.
/// </summary>
internal sealed class VelopackUpdateManagerAdapter : IUpdateManagerAdapter
{
    private readonly UpdateManager _updateManager;
    private UpdateInfo? _lastUpdateInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="VelopackUpdateManagerAdapter"/> class.
    /// </summary>
    /// <param name="githubRepoUri">GitHub repository URI for update source.</param>
    public VelopackUpdateManagerAdapter(Uri githubRepoUri)
    {
        ArgumentNullException.ThrowIfNull(githubRepoUri);
        _updateManager = new UpdateManager(new GithubSource(githubRepoUri.ToString(), null, false));
    }

    /// <inheritdoc />
    public bool IsInstalled => _updateManager.IsInstalled;

    /// <inheritdoc />
    public string? CurrentVersion => _updateManager.IsInstalled
        ? _updateManager.CurrentVersion?.ToString()
        : null;

    /// <inheritdoc />
    public async Task<AppUpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        var updateInfo = await _updateManager.CheckForUpdatesAsync().ConfigureAwait(false);

        if (updateInfo is null)
        {
            return null;
        }

        _lastUpdateInfo = updateInfo;

        return new AppUpdateInfo
        {
            TargetVersion = updateInfo.TargetFullRelease.Version.ToString(),
            CurrentVersion = CurrentVersion ?? "0.0.0",
            FileName = updateInfo.TargetFullRelease.FileName,
            Size = updateInfo.TargetFullRelease.Size,
            IsDowngrade = updateInfo.IsDowngrade,
        };
    }

    /// <inheritdoc />
    public async Task DownloadUpdateAsync(
        AppUpdateInfo updateInfo,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        // Re-check if cached info is stale
        _lastUpdateInfo ??= await _updateManager.CheckForUpdatesAsync().ConfigureAwait(false);

        if (_lastUpdateInfo is not null)
        {
            await _updateManager.DownloadUpdatesAsync(
                _lastUpdateInfo,
                progress: percentage => progress?.Report(percentage),
                cancelToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void ApplyUpdateAndRestart(string[]? restartArgs = null)
    {
        _updateManager.ApplyUpdatesAndRestart(_lastUpdateInfo?.TargetFullRelease, restartArgs);
    }

    /// <inheritdoc />
    public void ApplyUpdateOnExit()
    {
        _updateManager.ApplyUpdatesAndExit(_lastUpdateInfo?.TargetFullRelease);
    }
}
