// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Detects Star Citizen game installations by scanning the file system.
/// </summary>
public sealed class GameDetectionService : IGameDetectionService
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameDetectionService"/> class.
    /// </summary>
    /// <param name="fileSystem">File system abstraction.</param>
    public GameDetectionService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public IReadOnlyList<GameInstallation> DetectInstallations(string gameFolderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameFolderPath);

        var installations = new List<GameInstallation>();

        foreach (var mode in Enum.GetValues<GameMode>())
        {
            var modePath = GameConstants.GetGameModePath(gameFolderPath, mode);
            var exePath = GameConstants.GetExePath(modePath);

            if (!_fileSystem.DirectoryExists(modePath) || !_fileSystem.FileExists(exePath))
            {
                continue;
            }

            var version = _fileSystem.GetFileVersion(exePath);

            installations.Add(new GameInstallation
            {
                Mode = mode,
                RootFolderPath = modePath,
                ExeFilePath = exePath,
                ExeVersion = version,
            });
        }

        return installations;
    }

    /// <inheritdoc />
    public string? FindGameFolder(string searchPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPath);

        // Direct match: path already contains game mode folders.
        if (HasAnyGameMode(searchPath))
        {
            return searchPath;
        }

        var folderName = Path.GetFileName(searchPath);

        // User pointed at Bin64 -> go up 2 levels (Bin64 -> ModeFolder -> GameFolder).
        if (string.Equals(folderName, GameConstants.BinFolderName, StringComparison.OrdinalIgnoreCase))
        {
            var modeFolder = Path.GetDirectoryName(searchPath);
            var gameFolder = modeFolder is null ? null : Path.GetDirectoryName(modeFolder);
            if (gameFolder is not null && HasAnyGameMode(gameFolder))
            {
                return gameFolder;
            }
        }

        // User pointed at a mode folder (LIVE/PTU/EPTU) -> go up 1 level.
        if (IsGameModeFolderName(folderName))
        {
            var parent = Path.GetDirectoryName(searchPath);
            if (parent is not null && HasAnyGameMode(parent))
            {
                return parent;
            }
        }

        // Check for a StarCitizen subfolder.
        var scSubfolder = Path.Combine(searchPath, GameConstants.GameFolderName);
        if (_fileSystem.DirectoryExists(scSubfolder) && HasAnyGameMode(scSubfolder))
        {
            return scSubfolder;
        }

        return null;
    }

    private static bool IsGameModeFolderName(string? folderName)
    {
        if (string.IsNullOrEmpty(folderName))
        {
            return false;
        }

        foreach (var mode in Enum.GetValues<GameMode>())
        {
            if (string.Equals(folderName, GameConstants.GetModeFolderName(mode), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAnyGameMode(string folderPath)
    {
        foreach (var mode in Enum.GetValues<GameMode>())
        {
            var modePath = GameConstants.GetGameModePath(folderPath, mode);
            if (_fileSystem.DirectoryExists(modePath))
            {
                return true;
            }
        }

        return false;
    }
}
