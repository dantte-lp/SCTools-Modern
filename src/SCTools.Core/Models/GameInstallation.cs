// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Represents a detected Star Citizen game installation for a specific mode.
/// </summary>
public sealed record GameInstallation
{
    /// <summary>Gets the game mode (Live, PTU, EPTU).</summary>
    public required GameMode Mode { get; init; }

    /// <summary>Gets the root folder path for this mode (e.g. .../StarCitizen/LIVE).</summary>
    public required string RootFolderPath { get; init; }

    /// <summary>Gets the full path to the game executable.</summary>
    public required string ExeFilePath { get; init; }

    /// <summary>Gets the executable file version, or <c>null</c> if unavailable.</summary>
    public string? ExeVersion { get; init; }
}
