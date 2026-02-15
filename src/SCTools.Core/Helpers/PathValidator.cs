// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Helpers;

/// <summary>
/// Validates and sanitizes file paths to prevent directory traversal attacks.
/// </summary>
public static class PathValidator
{
    /// <summary>
    /// Safely combines a base directory with a relative path, ensuring the result
    /// stays within the base directory. Prevents directory traversal via ".." segments.
    /// </summary>
    /// <param name="basePath">The trusted base directory path.</param>
    /// <param name="relativePath">The untrusted relative path to combine.</param>
    /// <returns>The validated full path.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="relativePath"/> escapes <paramref name="basePath"/>.
    /// </exception>
    public static string SafeCombine(string basePath, string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var fullBase = Path.GetFullPath(basePath);
        if (!fullBase.EndsWith(Path.DirectorySeparatorChar))
        {
            fullBase += Path.DirectorySeparatorChar;
        }

        var combined = Path.GetFullPath(Path.Combine(fullBase, relativePath));

        if (!combined.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Path '{relativePath}' escapes base directory '{basePath}'.",
                nameof(relativePath));
        }

        return combined;
    }

    /// <summary>
    /// Checks whether a relative path is safe (does not escape the base directory).
    /// </summary>
    /// <param name="basePath">The trusted base directory path.</param>
    /// <param name="relativePath">The untrusted relative path to validate.</param>
    /// <returns><c>true</c> if the path is safe; otherwise <c>false</c>.</returns>
    public static bool IsSafePath(string basePath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        try
        {
            SafeCombine(basePath, relativePath);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
