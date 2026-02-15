// Licensed to the SCTools project under the MIT license.

using System.Net.Http.Headers;

namespace SCTools.Core.Helpers;

/// <summary>
/// Validates HTTP Content-Disposition headers and extracted file names
/// for security and correctness.
/// </summary>
public static class ContentValidator
{
    /// <summary>
    /// Allowed file extensions for download assets.
    /// </summary>
    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ini",
        ".zip",
        ".7z",
        ".tar",
        ".gz",
    };

    /// <summary>
    /// Extracts and validates the file name from a Content-Disposition header.
    /// </summary>
    /// <param name="contentDisposition">The Content-Disposition header value.</param>
    /// <returns>The validated file name, or <c>null</c> if invalid or missing.</returns>
    public static string? GetSafeFileName(ContentDispositionHeaderValue? contentDisposition)
    {
        if (contentDisposition is null)
        {
            return null;
        }

        var fileName = contentDisposition.FileNameStar ?? contentDisposition.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        // Remove surrounding quotes if present.
        fileName = fileName.Trim('"');

        return IsValidFileName(fileName) ? fileName : null;
    }

    /// <summary>
    /// Extracts and validates the file name from a download URL.
    /// </summary>
    /// <param name="downloadUrl">The download URL.</param>
    /// <returns>The validated file name, or <c>null</c> if invalid.</returns>
    public static string? GetSafeFileNameFromUrl(Uri? downloadUrl)
    {
        if (downloadUrl is null)
        {
            return null;
        }

        var segments = downloadUrl.AbsolutePath.Split('/');
        var lastSegment = segments.LastOrDefault();

        if (string.IsNullOrWhiteSpace(lastSegment))
        {
            return null;
        }

        // Remove query string artifacts.
        var qIndex = lastSegment.IndexOf('?', StringComparison.Ordinal);
        if (qIndex >= 0)
        {
            lastSegment = lastSegment[..qIndex];
        }

        return IsValidFileName(lastSegment) ? lastSegment : null;
    }

    /// <summary>
    /// Validates that a file name is safe (no path traversal, valid extension).
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns><c>true</c> if the file name is safe; otherwise <c>false</c>.</returns>
    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        // Strip to just the filename component to prevent path traversal.
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName) || safeName != fileName)
        {
            return false;
        }

        // Check for path traversal patterns.
        if (safeName.Contains("..", StringComparison.Ordinal) ||
            safeName.Contains('/', StringComparison.Ordinal) ||
            safeName.Contains('\\', StringComparison.Ordinal))
        {
            return false;
        }

        // Check for allowed extensions.
        var extension = Path.GetExtension(safeName);
        return _allowedExtensions.Contains(extension);
    }
}
