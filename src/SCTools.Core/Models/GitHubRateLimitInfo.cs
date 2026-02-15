// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Represents the current GitHub API rate limit status.
/// </summary>
public sealed record GitHubRateLimitInfo
{
    /// <summary>
    /// Gets the maximum number of requests per hour.
    /// </summary>
    public required int Limit { get; init; }

    /// <summary>
    /// Gets the number of remaining requests in the current window.
    /// </summary>
    public required int Remaining { get; init; }

    /// <summary>
    /// Gets the UTC time when the rate limit resets.
    /// </summary>
    public required DateTimeOffset ResetTime { get; init; }
}
