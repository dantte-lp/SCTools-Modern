// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Tracks file hashes for incremental update comparison using SHA-256.
/// </summary>
public interface IFileIndexService
{
    /// <summary>
    /// Builds a file index by scanning a directory and computing SHA-256 hashes.
    /// </summary>
    /// <param name="directoryPath">The directory to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary mapping relative paths to their hash entries.</returns>
    Task<Dictionary<string, FileHashEntry>> BuildIndexAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the SHA-256 hash of a single file.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Hex-encoded SHA-256 hash string.</returns>
    Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two file indexes and returns the list of files that need updating.
    /// </summary>
    /// <param name="localIndex">The current local file index.</param>
    /// <param name="remoteIndex">The target remote file index.</param>
    /// <returns>List of relative paths that are new or changed in the remote index.</returns>
    IReadOnlyList<string> GetChangedFiles(
        Dictionary<string, FileHashEntry> localIndex,
        Dictionary<string, FileHashEntry> remoteIndex);

    /// <summary>
    /// Saves a file index to a JSON file.
    /// </summary>
    /// <param name="index">The file index to save.</param>
    /// <param name="filePath">Path to write the JSON file.</param>
    void SaveIndex(Dictionary<string, FileHashEntry> index, string filePath);

    /// <summary>
    /// Loads a file index from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>The loaded file index, or an empty dictionary if the file does not exist.</returns>
    Dictionary<string, FileHashEntry> LoadIndex(string filePath);
}
