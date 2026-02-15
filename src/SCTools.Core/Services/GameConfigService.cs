// Licensed to the SCTools project under the MIT license.

using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;

namespace SCTools.Core.Services;

/// <summary>
/// Manages Star Citizen user.cfg files using simple key=value parsing.
/// Preserves comments and blank lines when modifying existing files.
/// </summary>
public sealed class GameConfigService : IGameConfigService
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameConfigService"/> class.
    /// </summary>
    /// <param name="fileSystem">File system abstraction.</param>
    public GameConfigService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public string? GetValue(string gameModePath, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cfgPath = GameConstants.GetUserConfigPath(gameModePath);
        if (!_fileSystem.FileExists(cfgPath))
        {
            return null;
        }

        var content = _fileSystem.ReadAllText(cfgPath);
        return FindValue(content, key);
    }

    /// <inheritdoc />
    public void SetValue(string gameModePath, string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var cfgPath = GameConstants.GetUserConfigPath(gameModePath);
        var content = _fileSystem.FileExists(cfgPath) ? _fileSystem.ReadAllText(cfgPath) : string.Empty;

        var updated = SetValueInContent(content, key, value);
        _fileSystem.WriteAllText(cfgPath, updated);
    }

    /// <inheritdoc />
    public bool RemoveValue(string gameModePath, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameModePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cfgPath = GameConstants.GetUserConfigPath(gameModePath);
        if (!_fileSystem.FileExists(cfgPath))
        {
            return false;
        }

        var content = _fileSystem.ReadAllText(cfgPath);
        var (removed, updated) = RemoveValueFromContent(content, key);

        if (removed)
        {
            _fileSystem.WriteAllText(cfgPath, updated);
        }

        return removed;
    }

    /// <summary>Finds a value by key in cfg file content.</summary>
    /// <param name="content">The cfg file content.</param>
    /// <param name="key">The key to search for.</param>
    /// <returns>The value, or <c>null</c> if not found.</returns>
    internal static string? FindValue(string content, string key)
    {
        using var reader = new StringReader(content);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.TrimStart();
            if (IsComment(trimmed))
            {
                continue;
            }

            var eqIndex = trimmed.IndexOf('=', StringComparison.Ordinal);
            if (eqIndex <= 0)
            {
                continue;
            }

            var lineKey = trimmed[..eqIndex].Trim();
            if (string.Equals(lineKey, key, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[(eqIndex + 1)..].Trim();
            }
        }

        return null;
    }

    /// <summary>Sets or updates a key=value pair in cfg content.</summary>
    /// <param name="content">The cfg file content.</param>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>Updated content string.</returns>
    internal static string SetValueInContent(string content, string key, string value)
    {
        var lines = SplitLines(content);
        var found = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (IsComment(trimmed))
            {
                continue;
            }

            var eqIndex = trimmed.IndexOf('=', StringComparison.Ordinal);
            if (eqIndex <= 0)
            {
                continue;
            }

            var lineKey = trimmed[..eqIndex].Trim();
            if (string.Equals(lineKey, key, StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"{key} = {value}";
                found = true;
                break;
            }
        }

        if (!found)
        {
            lines.Add($"{key} = {value}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>Removes a key=value pair from cfg content.</summary>
    /// <param name="content">The cfg file content.</param>
    /// <param name="key">The key to remove.</param>
    /// <returns>Tuple of (was removed, updated content).</returns>
    internal static (bool Removed, string Content) RemoveValueFromContent(string content, string key)
    {
        var lines = SplitLines(content);
        var removed = false;

        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var trimmed = lines[i].TrimStart();
            if (IsComment(trimmed))
            {
                continue;
            }

            var eqIndex = trimmed.IndexOf('=', StringComparison.Ordinal);
            if (eqIndex <= 0)
            {
                continue;
            }

            var lineKey = trimmed[..eqIndex].Trim();
            if (string.Equals(lineKey, key, StringComparison.OrdinalIgnoreCase))
            {
                lines.RemoveAt(i);
                removed = true;
                break;
            }
        }

        return (removed, string.Join(Environment.NewLine, lines));
    }

    private static bool IsComment(string trimmedLine) =>
        trimmedLine.Length == 0 ||
        trimmedLine.StartsWith(';') ||
        trimmedLine.StartsWith("//", StringComparison.Ordinal) ||
        trimmedLine.StartsWith("--", StringComparison.Ordinal);

    private static List<string> SplitLines(string content) =>
        [.. content.Split(["\r\n", "\n"], StringSplitOptions.None)];
}
