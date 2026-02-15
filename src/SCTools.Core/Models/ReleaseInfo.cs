// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Represents a GitHub release with its metadata and download information.
/// </summary>
public sealed record ReleaseInfo
{
    /// <summary>
    /// Gets the release tag name (e.g. "v3.24.0").
    /// </summary>
    public required string TagName { get; init; }

    /// <summary>
    /// Gets the release display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the release body/description (Markdown).
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Gets the date when the release was published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a pre-release.
    /// </summary>
    public bool IsPreRelease { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a draft release.
    /// </summary>
    public bool IsDraft { get; init; }

    /// <summary>
    /// Gets the URL to the release page on GitHub.
    /// </summary>
    public Uri? HtmlUrl { get; init; }

    /// <summary>
    /// Gets the assets attached to this release.
    /// </summary>
    public IReadOnlyList<ReleaseAssetInfo> Assets { get; init; } = [];
}
