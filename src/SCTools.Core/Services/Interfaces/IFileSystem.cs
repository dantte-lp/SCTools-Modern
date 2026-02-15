// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Minimal file system abstraction for testability.
/// </summary>
public interface IFileSystem
{
    /// <summary>Checks whether the specified directory exists.</summary>
    /// <param name="path">Path to the directory.</param>
    /// <returns><c>true</c> if the directory exists; otherwise <c>false</c>.</returns>
    bool DirectoryExists(string path);

    /// <summary>Checks whether the specified file exists.</summary>
    /// <param name="path">Path to the file.</param>
    /// <returns><c>true</c> if the file exists; otherwise <c>false</c>.</returns>
    bool FileExists(string path);

    /// <summary>Gets the file version string for an executable.</summary>
    /// <param name="filePath">Path to the executable.</param>
    /// <returns>Version string, or <c>null</c> if unavailable.</returns>
    string? GetFileVersion(string filePath);
}
