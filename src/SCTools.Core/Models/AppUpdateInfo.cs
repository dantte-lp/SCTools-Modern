// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Information about an available application update.
/// </summary>
public sealed record AppUpdateInfo
{
    /// <summary>
    /// Gets the target version to update to.
    /// </summary>
    public required string TargetVersion { get; init; }

    /// <summary>
    /// Gets the currently installed version.
    /// </summary>
    public required string CurrentVersion { get; init; }

    /// <summary>
    /// Gets the file name of the update package.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the size of the update package in bytes.
    /// </summary>
    public long? Size { get; init; }

    /// <summary>
    /// Gets the release notes in Markdown format.
    /// </summary>
    public string? ReleaseNotes { get; init; }

    /// <summary>
    /// Gets a value indicating whether this update is a downgrade.
    /// </summary>
    public bool IsDowngrade { get; init; }

    /// <summary>
    /// Gets a value indicating whether delta updates are available.
    /// </summary>
    public bool HasDeltaUpdates { get; init; }
}
