// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// The current status of the auto-update process.
/// </summary>
public enum UpdateStatus
{
    /// <summary>No update activity in progress.</summary>
    Idle,

    /// <summary>Currently checking for available updates.</summary>
    Checking,

    /// <summary>An update is available and ready to download.</summary>
    UpdateAvailable,

    /// <summary>Currently downloading the update.</summary>
    Downloading,

    /// <summary>Download complete, ready to install.</summary>
    ReadyToInstall,

    /// <summary>The application is up to date.</summary>
    UpToDate,

    /// <summary>An error occurred during the update process.</summary>
    Error,

    /// <summary>The application is not installed (running from IDE or portable).</summary>
    NotInstalled,
}
