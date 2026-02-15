// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Manages installation and removal of language packs for Star Citizen.
/// </summary>
public interface ILanguagePackService
{
    /// <summary>
    /// Gets the list of installed language packs for a game mode directory.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory (e.g. .../LIVE).</param>
    /// <returns>List of installed language packs.</returns>
    IReadOnlyList<LanguagePack> GetInstalledLanguages(string gameModePath);

    /// <summary>
    /// Gets the currently selected language from user.cfg.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <returns>Language code, or <c>null</c> if not set (defaults to English).</returns>
    string? GetCurrentLanguage(string gameModePath);

    /// <summary>
    /// Sets the active language in user.cfg (g_language and g_languageAudio keys).
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="languageCode">Language code to activate.</param>
    void SetCurrentLanguage(string gameModePath, string languageCode);

    /// <summary>
    /// Removes the language setting from user.cfg, reverting to English.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    void ResetLanguage(string gameModePath);

    /// <summary>
    /// Installs a language pack from a source directory containing a global.ini file.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="languageCode">Language code (e.g. "russian").</param>
    /// <param name="sourceGlobalIniPath">Path to the source global.ini file.</param>
    /// <returns>Installation result status.</returns>
    InstallStatus InstallFromFile(string gameModePath, string languageCode, string sourceGlobalIniPath);

    /// <summary>
    /// Removes an installed language pack.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="languageCode">Language code to uninstall.</param>
    /// <returns><c>true</c> if the language was removed; <c>false</c> if it was not found.</returns>
    bool Uninstall(string gameModePath, string languageCode);

    /// <summary>
    /// Removes all non-English language packs and resets the language setting.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <returns><c>true</c> if any languages were removed.</returns>
    bool UninstallAll(string gameModePath);
}
