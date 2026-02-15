// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Detects Star Citizen installations on the local file system.
/// </summary>
public interface IGameDetectionService
{
    /// <summary>
    /// Detects all available game mode installations under the given root folder.
    /// </summary>
    /// <param name="gameFolderPath">Root StarCitizen folder (e.g. C:\Program Files\RSI\StarCitizen).</param>
    /// <returns>List of detected installations, possibly empty.</returns>
    IReadOnlyList<GameInstallation> DetectInstallations(string gameFolderPath);

    /// <summary>
    /// Searches for the StarCitizen game folder starting from the given path.
    /// Handles cases where the user selects a subfolder (Bin64, LIVE, etc.).
    /// </summary>
    /// <param name="searchPath">Starting path to search from.</param>
    /// <returns>Root game folder path, or <c>null</c> if not found.</returns>
    string? FindGameFolder(string searchPath);
}
