// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Helpers;

/// <summary>
/// Compares semantic version strings (e.g. "1.2.3", "v1.0.0-beta", "2.0.0+build42").
/// Supports optional "v" prefix, pre-release suffixes, and build metadata.
/// </summary>
public static class SemanticVersionComparer
{
    /// <summary>
    /// Compares two semantic version strings.
    /// </summary>
    /// <param name="version1">First version string.</param>
    /// <param name="version2">Second version string.</param>
    /// <returns>
    /// Negative if <paramref name="version1"/> is older,
    /// zero if equal,
    /// positive if <paramref name="version1"/> is newer.
    /// </returns>
    public static int Compare(string? version1, string? version2)
    {
        var v1 = Parse(version1);
        var v2 = Parse(version2);

        var majorDiff = v1.Major.CompareTo(v2.Major);
        if (majorDiff != 0)
        {
            return majorDiff;
        }

        var minorDiff = v1.Minor.CompareTo(v2.Minor);
        if (minorDiff != 0)
        {
            return minorDiff;
        }

        var patchDiff = v1.Patch.CompareTo(v2.Patch);
        if (patchDiff != 0)
        {
            return patchDiff;
        }

        // Pre-release versions have lower precedence than release versions.
        // e.g. "1.0.0-beta" < "1.0.0"
        return ComparePreRelease(v1.PreRelease, v2.PreRelease);
    }

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is newer than <paramref name="current"/>.
    /// </summary>
    /// <param name="current">The currently installed version.</param>
    /// <param name="candidate">The candidate version to compare.</param>
    /// <returns><c>true</c> if <paramref name="candidate"/> is newer; otherwise <c>false</c>.</returns>
    public static bool IsNewer(string? current, string? candidate)
    {
        return Compare(candidate, current) > 0;
    }

    /// <summary>
    /// Parses a semantic version string into its components.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
    /// <returns>Parsed version components.</returns>
    internal static ParsedVersion Parse(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return default;
        }

        var input = version.AsSpan().Trim();

        // Strip leading "v" or "V" prefix.
        if (input.Length > 0 && (input[0] == 'v' || input[0] == 'V'))
        {
            input = input[1..];
        }

        // Strip build metadata after "+".
        var plusIndex = input.IndexOf('+');
        if (plusIndex >= 0)
        {
            input = input[..plusIndex];
        }

        // Split pre-release after "-".
        string preRelease = string.Empty;
        var dashIndex = input.IndexOf('-');
        if (dashIndex >= 0)
        {
            preRelease = input[(dashIndex + 1)..].ToString();
            input = input[..dashIndex];
        }

        // Parse major.minor.patch.
        var parts = input.ToString().Split('.');
        var major = parts.Length > 0 && int.TryParse(parts[0], out var m) ? m : 0;
        var minor = parts.Length > 1 && int.TryParse(parts[1], out var n) ? n : 0;
        var patch = parts.Length > 2 && int.TryParse(parts[2], out var p) ? p : 0;

        return new ParsedVersion(major, minor, patch, preRelease);
    }

    private static int ComparePreRelease(string preRelease1, string preRelease2)
    {
        var hasPreRelease1 = !string.IsNullOrEmpty(preRelease1);
        var hasPreRelease2 = !string.IsNullOrEmpty(preRelease2);

        if (!hasPreRelease1 && !hasPreRelease2)
        {
            return 0;
        }

        // A version without pre-release has higher precedence.
        if (!hasPreRelease1)
        {
            return 1;
        }

        if (!hasPreRelease2)
        {
            return -1;
        }

        return string.Compare(preRelease1, preRelease2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Represents a parsed semantic version.
    /// </summary>
    internal readonly record struct ParsedVersion(int Major, int Minor, int Patch, string PreRelease);
}
