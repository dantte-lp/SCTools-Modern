// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Represents a downloadable asset attached to a GitHub release.
/// </summary>
public sealed record ReleaseAssetInfo
{
    /// <summary>
    /// Gets the asset file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the asset size in bytes.
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// Gets the browser download URL.
    /// </summary>
    public required Uri DownloadUrl { get; init; }

    /// <summary>
    /// Gets the content type of the asset.
    /// </summary>
    public string? ContentType { get; init; }
}
