// Licensed to the SCTools project under the MIT license.

namespace SCTools.Core.Models;

/// <summary>
/// Result status of a language pack installation or uninstallation operation.
/// </summary>
public enum InstallStatus
{
    /// <summary>Operation completed successfully.</summary>
    Success,

    /// <summary>The archive file is invalid or corrupt.</summary>
    PackageError,

    /// <summary>File I/O operation failed.</summary>
    FileError,

    /// <summary>Path validation failed (possible traversal attack).</summary>
    PathError,

    /// <summary>The target game installation was not found.</summary>
    GameNotFound,
}
