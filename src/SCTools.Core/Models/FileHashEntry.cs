// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Represents a file's hash information for incremental update tracking.
/// </summary>
public sealed record FileHashEntry
{
    /// <summary>
    /// Gets the relative file path (forward-slash separated).
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the SHA-256 hash of the file content as a hex string.
    /// </summary>
    public required string Hash { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public required long Size { get; init; }
}
