// Licensed to the SCTools project under the MIT license.

using System.Diagnostics;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Default file system implementation backed by <see cref="System.IO"/>.
/// </summary>
public sealed class PhysicalFileSystem : IFileSystem
{
    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public string? GetFileVersion(string filePath)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
        return versionInfo.FileVersion?.Replace(',', '.');
    }

    /// <inheritdoc />
    public string ReadAllText(string filePath) => File.ReadAllText(filePath);

    /// <inheritdoc />
    public void WriteAllText(string filePath, string content) => File.WriteAllText(filePath, content);

    /// <inheritdoc />
    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    /// <inheritdoc />
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <inheritdoc />
    public void DeleteDirectory(string path) => Directory.Delete(path, recursive: true);

    /// <inheritdoc />
    public void DeleteFile(string path) => File.Delete(path);

    /// <inheritdoc />
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite) =>
        File.Copy(sourcePath, destinationPath, overwrite);
}
