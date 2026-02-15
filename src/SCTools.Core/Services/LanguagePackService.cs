// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Helpers;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Manages installation and removal of Star Citizen language packs.
/// Handles localization file placement, user.cfg language settings,
/// and path traversal protection.
/// </summary>
public sealed class LanguagePackService : ILanguagePackService
{
    private readonly IFileSystem _fileSystem;
    private readonly IGameConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguagePackService"/> class.
    /// </summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="configService">Game config service for user.cfg management.</param>
    public LanguagePackService(IFileSystem fileSystem, IGameConfigService configService)
    {
        _fileSystem = fileSystem;
        _configService = configService;
    }

    /// <inheritdoc />
    public IReadOnlyList<LanguagePack> GetInstalledLanguages(string gameModePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);

        var locPath = GameConstants.GetLocalizationPath(gameModePath);
        if (!_fileSystem.DirectoryExists(locPath))
        {
            return [];
        }

        var packs = new List<LanguagePack>();
        foreach (var dir in _fileSystem.GetDirectories(locPath))
        {
            var langCode = Path.GetFileName(dir);
            if (string.IsNullOrEmpty(langCode))
            {
                continue;
            }

            var globalIniPath = Path.Combine(dir, GameConstants.GlobalIniName);
            packs.Add(new LanguagePack
            {
                LanguageCode = langCode,
                FolderPath = dir,
                HasGlobalIni = _fileSystem.FileExists(globalIniPath),
            });
        }

        return packs;
    }

    /// <inheritdoc />
    public string? GetCurrentLanguage(string gameModePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        return _configService.GetValue(gameModePath, GameConstants.LanguageKey);
    }

    /// <inheritdoc />
    public void SetCurrentLanguage(string gameModePath, string languageCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        _configService.SetValue(gameModePath, GameConstants.LanguageKey, languageCode);
        _configService.SetValue(gameModePath, GameConstants.LanguageAudioKey, GameConstants.DefaultLanguage);
    }

    /// <inheritdoc />
    public void ResetLanguage(string gameModePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);

        _configService.RemoveValue(gameModePath, GameConstants.LanguageKey);
        _configService.RemoveValue(gameModePath, GameConstants.LanguageAudioKey);
    }

    /// <inheritdoc />
    public InstallStatus InstallFromFile(string gameModePath, string languageCode, string sourceGlobalIniPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceGlobalIniPath);

        if (!_fileSystem.DirectoryExists(gameModePath))
        {
            return InstallStatus.GameNotFound;
        }

        if (!_fileSystem.FileExists(sourceGlobalIniPath))
        {
            return InstallStatus.PackageError;
        }

        var locPath = GameConstants.GetLocalizationPath(gameModePath);

        if (!PathValidator.IsSafePath(gameModePath, Path.Combine(
                GameConstants.DataFolderName,
                GameConstants.LocalizationFolderName,
                languageCode,
                GameConstants.GlobalIniName)))
        {
            return InstallStatus.PathError;
        }

        try
        {
            var langFolder = Path.Combine(locPath, languageCode);
            _fileSystem.CreateDirectory(langFolder);

            var destIniPath = Path.Combine(langFolder, GameConstants.GlobalIniName);
            _fileSystem.CopyFile(sourceGlobalIniPath, destIniPath, overwrite: true);

            return InstallStatus.Success;
        }
        catch (IOException)
        {
            return InstallStatus.FileError;
        }
        catch (UnauthorizedAccessException)
        {
            return InstallStatus.FileError;
        }
    }

    /// <inheritdoc />
    public bool Uninstall(string gameModePath, string languageCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        if (string.Equals(languageCode, GameConstants.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var langFolder = Path.Combine(GameConstants.GetLocalizationPath(gameModePath), languageCode);
        if (!_fileSystem.DirectoryExists(langFolder))
        {
            return false;
        }

        _fileSystem.DeleteDirectory(langFolder);

        var currentLang = GetCurrentLanguage(gameModePath);
        if (string.Equals(currentLang, languageCode, StringComparison.OrdinalIgnoreCase))
        {
            ResetLanguage(gameModePath);
        }

        return true;
    }

    /// <inheritdoc />
    public bool UninstallAll(string gameModePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);

        var installed = GetInstalledLanguages(gameModePath);
        var anyRemoved = false;

        foreach (var pack in installed)
        {
            if (string.Equals(pack.LanguageCode, GameConstants.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (_fileSystem.DirectoryExists(pack.FolderPath))
            {
                _fileSystem.DeleteDirectory(pack.FolderPath);
                anyRemoved = true;
            }
        }

        if (anyRemoved)
        {
            ResetLanguage(gameModePath);
        }

        return anyRemoved;
    }
}
