// Licensed to the SCTools project under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace SCTools.Core.Models;

/// <summary>
/// Application settings bound from <c>appsettings.json</c> via <c>IOptions&lt;AppSettings&gt;</c>.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Configuration section name in appsettings.json.</summary>
    public const string SectionName = "App";

    /// <summary>Gets or sets the root StarCitizen game folder path.</summary>
    public string? GameFolder { get; set; }

    /// <summary>Gets or sets a value indicating whether the app starts minimized to tray.</summary>
    public bool RunMinimized { get; set; }

    /// <summary>Gets or sets a value indicating whether incremental (delta) downloads are used.</summary>
    public bool AllowIncrementalDownload { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether automatic update checks are enabled.</summary>
    public bool RegularCheckForUpdates { get; set; } = true;

    /// <summary>Gets or sets the interval (in minutes) between automatic update checks.</summary>
    [Range(1, 1440)]
    public int UpdateCheckIntervalMinutes { get; set; } = 60;
}
