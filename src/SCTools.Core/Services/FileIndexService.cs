// Licensed to the SCTools project under the MIT license.

using System.Security.Cryptography;
using System.Text.Json;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Tracks file hashes using SHA-256 for incremental update comparison.
/// </summary>
public sealed class FileIndexService : IFileIndexService
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileIndexService"/> class.
    /// </summary>
    /// <param name="fileSystem">File system abstraction.</param>
    public FileIndexService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, FileHashEntry>> BuildIndexAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var index = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase);

        if (!_fileSystem.DirectoryExists(directoryPath))
        {
            return index;
        }

        var files = _fileSystem.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        var basePath = Path.GetFullPath(directoryPath);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = Path.GetFullPath(file);
            var relativePath = Path.GetRelativePath(basePath, fullPath)
                .Replace('\\', '/');

            var hash = await ComputeHashAsync(fullPath, cancellationToken).ConfigureAwait(false);
            var size = _fileSystem.GetFileSize(fullPath);

            index[relativePath] = new FileHashEntry
            {
                RelativePath = relativePath,
                Hash = hash,
                Size = size,
            };
        }

        return index;
    }

    /// <inheritdoc />
    public async Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var stream = _fileSystem.OpenRead(filePath);
        await using (stream.ConfigureAwait(false))
        {
            var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            return Convert.ToHexString(hashBytes);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetChangedFiles(
        Dictionary<string, FileHashEntry> localIndex,
        Dictionary<string, FileHashEntry> remoteIndex)
    {
        ArgumentNullException.ThrowIfNull(localIndex);
        ArgumentNullException.ThrowIfNull(remoteIndex);

        var changed = new List<string>();

        foreach (var (relativePath, remoteEntry) in remoteIndex)
        {
            if (!localIndex.TryGetValue(relativePath, out var localEntry) ||
                !string.Equals(localEntry.Hash, remoteEntry.Hash, StringComparison.OrdinalIgnoreCase) ||
                localEntry.Size != remoteEntry.Size)
            {
                changed.Add(relativePath);
            }
        }

        return changed;
    }

    /// <inheritdoc />
    public void SaveIndex(Dictionary<string, FileHashEntry> index, string filePath)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = JsonSerializer.Serialize(
            index,
            FileIndexJsonContext.Default.DictionaryStringFileHashEntry);

        _fileSystem.WriteAllText(filePath, json);
    }

    /// <inheritdoc />
    public Dictionary<string, FileHashEntry> LoadIndex(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!_fileSystem.FileExists(filePath))
        {
            return new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase);
        }

        var json = _fileSystem.ReadAllText(filePath);
        var index = JsonSerializer.Deserialize(
            json,
            FileIndexJsonContext.Default.DictionaryStringFileHashEntry);

        return index ?? new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase);
    }
}
