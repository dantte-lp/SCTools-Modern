// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Constants and path helpers for Star Citizen game files.
/// </summary>
public static class GameConstants
{
    /// <summary>Game root folder name under the RSI Launcher directory.</summary>
    public const string GameFolderName = "StarCitizen";

    /// <summary>Binary folder inside each game mode directory.</summary>
    public const string BinFolderName = "Bin64";

    /// <summary>Data folder inside each game mode directory.</summary>
    public const string DataFolderName = "data";

    /// <summary>Localization subfolder inside the data directory.</summary>
    public const string LocalizationFolderName = "localization";

    /// <summary>Game executable file name.</summary>
    public const string GameExeName = "StarCitizen.exe";

    /// <summary>User configuration file name.</summary>
    public const string UserConfigName = "user.cfg";

    /// <summary>Global localization INI file name.</summary>
    public const string GlobalIniName = "global.ini";

    /// <summary>Config key for the text language setting.</summary>
    public const string LanguageKey = "g_language";

    /// <summary>Config key for the audio language setting.</summary>
    public const string LanguageAudioKey = "g_languageAudio";

    /// <summary>Default (English) localization name.</summary>
    public const string DefaultLanguage = "english";

    /// <summary>
    /// Returns the on-disk folder name for a <see cref="GameMode"/> (e.g. "LIVE").
    /// </summary>
    /// <param name="mode">The game mode.</param>
    /// <returns>Uppercase folder name.</returns>
    public static string GetModeFolderName(GameMode mode) =>
        mode.ToString().ToUpperInvariant();

    /// <summary>
    /// Combines the game root path with a mode folder name.
    /// </summary>
    /// <param name="gameFolderPath">Root StarCitizen folder.</param>
    /// <param name="mode">Target game mode.</param>
    /// <returns>Full path to the mode directory.</returns>
    public static string GetGameModePath(string gameFolderPath, GameMode mode) =>
        Path.Combine(gameFolderPath, GetModeFolderName(mode));

    /// <summary>
    /// Returns the full path to the game executable inside a mode folder.
    /// </summary>
    /// <param name="gameModePath">Path to the mode directory (e.g. .../LIVE).</param>
    /// <returns>Full path to StarCitizen.exe.</returns>
    public static string GetExePath(string gameModePath) =>
        Path.Combine(gameModePath, BinFolderName, GameExeName);

    /// <summary>
    /// Returns the full path to the data folder inside a mode directory.
    /// </summary>
    /// <param name="gameModePath">Path to the mode directory.</param>
    /// <returns>Full path to the data folder.</returns>
    public static string GetDataPath(string gameModePath) =>
        Path.Combine(gameModePath, DataFolderName);

    /// <summary>
    /// Returns the full path to the localization folder inside a mode directory.
    /// </summary>
    /// <param name="gameModePath">Path to the mode directory.</param>
    /// <returns>Full path to the localization folder.</returns>
    public static string GetLocalizationPath(string gameModePath) =>
        Path.Combine(gameModePath, DataFolderName, LocalizationFolderName);

    /// <summary>
    /// Returns the full path to the user.cfg file inside a mode directory.
    /// </summary>
    /// <param name="gameModePath">Path to the mode directory.</param>
    /// <returns>Full path to user.cfg.</returns>
    public static string GetUserConfigPath(string gameModePath) =>
        Path.Combine(gameModePath, UserConfigName);
}
