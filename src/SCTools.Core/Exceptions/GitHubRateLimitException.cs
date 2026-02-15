// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Exceptions;

/// <summary>
/// Thrown when the GitHub API rate limit has been exceeded.
/// </summary>
public sealed class GitHubRateLimitException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubRateLimitException"/> class.
    /// </summary>
    public GitHubRateLimitException()
        : base("GitHub API rate limit exceeded.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubRateLimitException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    public GitHubRateLimitException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubRateLimitException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public GitHubRateLimitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubRateLimitException"/> class.
    /// </summary>
    /// <param name="resetTime">UTC time when the rate limit resets.</param>
    /// <param name="limit">Maximum number of requests per hour.</param>
    /// <param name="remaining">Number of requests remaining (typically 0).</param>
    public GitHubRateLimitException(DateTimeOffset resetTime, int limit, int remaining)
        : base($"GitHub API rate limit exceeded ({remaining}/{limit}). Resets at {resetTime:HH:mm:ss} UTC.")
    {
        ResetTime = resetTime;
        Limit = limit;
        Remaining = remaining;
    }

    /// <summary>
    /// Gets the UTC time when the rate limit resets.
    /// </summary>
    public DateTimeOffset ResetTime { get; }

    /// <summary>
    /// Gets the maximum number of requests per hour.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Gets the number of requests remaining.
    /// </summary>
    public int Remaining { get; }
}
