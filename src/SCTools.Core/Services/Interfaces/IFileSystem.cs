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

    /// <summary>Reads all text from a file.</summary>
    /// <param name="filePath">Path to the file.</param>
    /// <returns>File contents as a string.</returns>
    string ReadAllText(string filePath);

    /// <summary>Writes text to a file, creating or overwriting it.</summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="content">Text content to write.</param>
    void WriteAllText(string filePath, string content);

    /// <summary>Returns the names of subdirectories in the specified directory.</summary>
    /// <param name="path">Path to the directory.</param>
    /// <returns>Array of subdirectory full paths.</returns>
    string[] GetDirectories(string path);

    /// <summary>Creates all directories in the specified path.</summary>
    /// <param name="path">The directory path to create.</param>
    void CreateDirectory(string path);

    /// <summary>Deletes a directory and all its contents recursively.</summary>
    /// <param name="path">Path to the directory to delete.</param>
    void DeleteDirectory(string path);

    /// <summary>Deletes a file.</summary>
    /// <param name="path">Path to the file to delete.</param>
    void DeleteFile(string path);

    /// <summary>Copies a file to a new location.</summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="destinationPath">Destination file path.</param>
    /// <param name="overwrite">Whether to overwrite existing file.</param>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);
}
