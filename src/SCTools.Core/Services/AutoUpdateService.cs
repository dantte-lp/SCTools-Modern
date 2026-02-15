// Licensed to the SCTools project under the MIT license.

using Microsoft.Extensions.Logging;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Manages automatic application updates using the Velopack framework.
/// This class wraps the update manager behind <see cref="IAutoUpdateService"/>
/// for testability. The actual Velopack <c>UpdateManager</c> is injected via
/// <see cref="IUpdateManagerAdapter"/> to keep Core cross-platform.
/// </summary>
public sealed partial class AutoUpdateService : IAutoUpdateService
{
    private readonly IUpdateManagerAdapter _updateManager;
    private readonly ILogger<AutoUpdateService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoUpdateService"/> class.
    /// </summary>
    /// <param name="updateManager">The update manager adapter.</param>
    /// <param name="logger">Logger instance.</param>
    public AutoUpdateService(IUpdateManagerAdapter updateManager, ILogger<AutoUpdateService> logger)
    {
        _updateManager = updateManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsInstalled => _updateManager.IsInstalled;

    /// <inheritdoc />
    public string? CurrentVersion => _updateManager.CurrentVersion;

    /// <inheritdoc />
    public async Task<AppUpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (!_updateManager.IsInstalled)
        {
            LogNotInstalled();
            return null;
        }

        LogCheckingForUpdates();

        try
        {
            var updateInfo = await _updateManager.CheckForUpdateAsync(cancellationToken)
                .ConfigureAwait(false);

            if (updateInfo is null)
            {
                LogUpToDate(CurrentVersion ?? "unknown");
                return null;
            }

            LogUpdateAvailable(CurrentVersion ?? "unknown", updateInfo.TargetVersion);

            return updateInfo;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogUpdateCheckFailed(ex);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task DownloadUpdateAsync(
        AppUpdateInfo updateInfo,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        LogDownloading(updateInfo.TargetVersion);

        await _updateManager.DownloadUpdateAsync(updateInfo, progress, cancellationToken)
            .ConfigureAwait(false);

        LogDownloaded(updateInfo.TargetVersion);
    }

    /// <inheritdoc />
    public void ApplyUpdateAndRestart(string[]? restartArgs = null)
    {
        LogApplyingRestart();
        _updateManager.ApplyUpdateAndRestart(restartArgs);
    }

    /// <inheritdoc />
    public void ApplyUpdateOnExit()
    {
        LogApplyingOnExit();
        _updateManager.ApplyUpdateOnExit();
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Application is not installed, skipping update check")]
    private partial void LogNotInstalled();

    [LoggerMessage(Level = LogLevel.Information, Message = "Checking for updates...")]
    private partial void LogCheckingForUpdates();

    [LoggerMessage(Level = LogLevel.Information, Message = "Application is up to date (v{version})")]
    private partial void LogUpToDate(string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Update available: v{current} -> v{target}")]
    private partial void LogUpdateAvailable(string current, string target);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to check for updates")]
    private partial void LogUpdateCheckFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Downloading update v{version}...")]
    private partial void LogDownloading(string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Update v{version} downloaded successfully")]
    private partial void LogDownloaded(string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying update and restarting...")]
    private partial void LogApplyingRestart();

    [LoggerMessage(Level = LogLevel.Information, Message = "Update will be applied on exit")]
    private partial void LogApplyingOnExit();
}
