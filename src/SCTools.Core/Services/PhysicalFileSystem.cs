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
}
