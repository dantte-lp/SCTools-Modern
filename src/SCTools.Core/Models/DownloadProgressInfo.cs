// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Progress information for a download operation.
/// </summary>
public sealed record DownloadProgressInfo
{
    /// <summary>
    /// Gets the total number of bytes to download, or <c>null</c> if unknown.
    /// </summary>
    public long? TotalBytes { get; init; }

    /// <summary>
    /// Gets the number of bytes downloaded so far.
    /// </summary>
    public long BytesDownloaded { get; init; }

    /// <summary>
    /// Gets the current file being processed, or <c>null</c> if not applicable.
    /// </summary>
    public string? CurrentFile { get; init; }

    /// <summary>
    /// Gets the download progress as a fraction between 0.0 and 1.0,
    /// or <c>null</c> if total size is unknown.
    /// </summary>
    public double? ProgressFraction =>
        TotalBytes is > 0 ? (double)BytesDownloaded / TotalBytes.Value : null;
}
