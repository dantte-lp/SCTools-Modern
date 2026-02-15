// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Services.Interfaces;

/// <summary>
/// Manages reading and writing Star Citizen user.cfg key=value configuration files.
/// </summary>
public interface IGameConfigService
{
    /// <summary>
    /// Gets a configuration value from user.cfg.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="key">Configuration key to look up.</param>
    /// <returns>The value, or <c>null</c> if the key does not exist or the file is missing.</returns>
    string? GetValue(string gameModePath, string key);

    /// <summary>
    /// Sets a configuration value in user.cfg. Creates the file if it does not exist.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="key">Configuration key.</param>
    /// <param name="value">Configuration value.</param>
    void SetValue(string gameModePath, string key, string value);

    /// <summary>
    /// Removes a configuration key from user.cfg.
    /// </summary>
    /// <param name="gameModePath">Path to the game mode directory.</param>
    /// <param name="key">Configuration key to remove.</param>
    /// <returns><c>true</c> if the key was found and removed; otherwise <c>false</c>.</returns>
    bool RemoveValue(string gameModePath, string key);
}
