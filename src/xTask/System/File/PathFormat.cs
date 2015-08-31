// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem
{
    /// <summary>
    /// The various path formats.
    /// </summary>
    public enum PathFormat
    {
        /// <summary>
        /// Unknown (and possibly invalid) format
        /// </summary>
        UnknownFormat,

        /// <summary>
        /// Fully qualified against a specific drive. (C:\rest_of_path)
        /// </summary>
        DriveAbsolute,

        /// <summary>
        /// Relative to the current directory on the specified drive. (C:rest_of_path)
        /// </summary>
        DriveRelative,

        /// <summary>
        /// Rooted to the current drive. (\rest_of_path)
        /// </summary>
        CurrentVolumeRelative,

        /// <summary>
        /// Extended length format. (\\?\C:\rest_of_path, \\?\HarddiskVolume1\, etc.)
        /// </summary>
        VolumeAbsoluteExtended,

        /// <summary>
        /// Device syntax. (\\.\COM1)
        /// </summary>
        Device,

        /// <summary>
        /// Relative to the current working directory (rest_of_path)
        /// </summary>
        CurrentDirectoryRelative,

        /// <summary>
        /// UNC (\\server\share)
        /// </summary>
        UniformNamingConvention,

        /// <summary>
        /// UNC extended length format (\\?\UNC\Server\Share)
        /// </summary>
        UniformNamingConventionExtended
    }
}