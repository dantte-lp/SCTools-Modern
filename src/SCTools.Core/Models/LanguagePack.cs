// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Represents an installed or available language pack for Star Citizen.
/// </summary>
public sealed record LanguagePack
{
    /// <summary>
    /// Gets the language code (e.g. "russian", "german", "korean").
    /// </summary>
    public required string LanguageCode { get; init; }

    /// <summary>
    /// Gets the version tag of the installed pack, or <c>null</c> if not tracked.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets the full path to the language folder (e.g. .../data/localization/russian).
    /// </summary>
    public required string FolderPath { get; init; }

    /// <summary>
    /// Gets a value indicating whether the global.ini file exists for this language.
    /// </summary>
    public bool HasGlobalIni { get; init; }
}
