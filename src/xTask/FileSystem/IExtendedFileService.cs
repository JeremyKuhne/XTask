// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// Less commonly used file related APIs.
    /// </summary>
    /// <remarks>
    /// One of the metrics for what goes in this interface is frequency of use. Another metric
    /// is whether or not the APIs already exist in System.IO.
    /// </remarks>
    public interface IExtendedFileService : IFileService
    {
        /// <summary>
        /// Gets the short path version of the given path, if it has one.
        /// </summary>
        string GetShortPath(string path);

        /// <summary>
        /// Gets the long path version of the given path (expands short path segments).
        /// </summary>
        string GetLongPath(string path);

        /// <summary>
        /// Gets the final path for the given path (case normalized).
        /// </summary>
        string GetFinalPath(string path);

        /// <summary>
        /// Returns the device name for the given DOS alias/symlink (as defined in \$GLOBAL).
        /// For example, "C:" will return something like "\Device\HarddiskVolume2".
        /// To return all DOS aliases, pass null. This list usually has several hundred
        /// aliases.
        /// </summary>
        /// <remarks>
        /// These aliases can be viewed using the WinObj SysInternals tool.
        /// </remarks>
        IEnumerable<string> QueryDosDeviceNames(string dosAlias);

        /// <summary>
        /// Gets the volume name (for example, \\?\Volume{86561e68-3ec0-11e3-be66-806e6f6e6963}\) for the given mount point.
        /// </summary>
        string GetVolumeName(string volumeMountPoint);

        /// <summary>
        /// Gets the volume mount point for the given path, which is either the folder where the drive is mounted
        /// (e.g. C:\MountedDrives\DDrive\) or the root of the volume (e.g. C:\).
        /// </summary>
        string GetVolumePathName(string path);

        /// <summary>
        /// Gets volume information for the specified root path.
        /// </summary>
        VolumeInformation GetVolumeInformation(string rootPath);

        /// <summary>
        /// Gets all of the defined mount points for the given volume name.
        /// </summary>
        IEnumerable<string> GetVolumePathNames(string volumeName);

        /// <summary>
        /// Returns drive strings in use ("C:", "D:", etc.)
        /// </summary>
        IEnumerable<string> GetLogicalDriveStrings();

        /// <summary>
        /// Returns alternate stream information for a file, if there is any.
        /// </summary>
        IEnumerable<AlternateStreamInformation> GetAlternateStreams(string path);
    }
}
