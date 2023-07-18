// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File;

/// <summary>
///  The various path formats.
/// </summary>
public enum PathFormat
{
    /// <summary>
    ///  Unknown (and possibly invalid) format
    /// </summary>
    UnknownFormat,

    /// <summary>
    ///  Fully qualified against a local path. (C:\rest_of_path)
    /// </summary>
    LocalFullyQualified,

    /// <summary>
    ///  Relative to the current directory on the specified drive. (C:rest_of_path)
    /// </summary>
    LocalDriveRooted,

    /// <summary>
    ///  Rooted to the current drive. (\rest_of_path)
    /// </summary>
    LocalCurrentDriveRooted,

    /// <summary>
    ///  Relative to the current working directory (rest_of_path)
    /// </summary>
    LocalCurrentDirectoryRelative,

    /// <summary>
    ///  UNC (\\server\share)
    /// </summary>
    UniformNamingConvention
}