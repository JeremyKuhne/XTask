// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class ExtendedFileServiceExtensions
    {
        public static string GetCanonicalRoot(this IExtendedFileService fileService, string path)
        {
            // Get the canonical volume path name for the given directory
            path = fileService.GetFullPath(path);
            int rootLength;
            var format = Paths.GetPathFormat(path, out rootLength);
            if (format == PathFormat.UnknownFormat)
            {
                throw new InvalidOperationException();
            }

            string root = path.Substring(0, rootLength);
            string simpleRoot = root;
            string canonicalRoot = root;

            switch (format)
            {
                case PathFormat.UniformNamingConventionExtended:
                    simpleRoot = @"\\" + root.Substring(Paths.ExtendedUncPrefix.Length);
                    goto case PathFormat.UniformNamingConvention;
                case PathFormat.UniformNamingConvention:
                    canonicalRoot = simpleRoot;
                    break;
                case PathFormat.VolumeAbsoluteExtended:
                case PathFormat.DriveAbsolute:
                    canonicalRoot = fileService.GetVolumeName(root);
                    simpleRoot = fileService.GetMountPoint(root);
                    break;
            }

            return canonicalRoot;
        }

        /// <summary>
        /// Return the legacy drive letter (e.g. "C", "D") for the given path or null if one doesn't exist
        /// </summary>
        public static string GetDriveLetter(this IExtendedFileService fileService, string path)
        {
            string pathCanonicalRoot = fileService.GetCanonicalRoot(path);

            // We have to walk drives
            foreach (var drive in fileService.GetLogicalDriveStrings())
            {
                string driveCanonicalRoot = fileService.GetCanonicalRoot(drive);
                if (String.Equals(pathCanonicalRoot, driveCanonicalRoot))
                {
                    return drive;
                }
            }

            return null;
            //int rootLength;
            //switch (Paths.GetPathFormat(path, out rootLength))
            //{
            //    case PathFormat.UnknownFormat:
            //    case PathFormat.CurrentDirectoryRelative:
            //    case PathFormat.CurrentVolumeRelative:
            //    case PathFormat.UniformNamingConvention:
            //    case PathFormat.UniformNamingConventionExtended:
            //        return null;
            //    case PathFormat.DriveAbsolute:
            //    case PathFormat.DriveRelative:
            //        // Already in the "C:" form
            //        return path.Substring(0, 1);
            //    case PathFormat.Device:
            //    case PathFormat.VolumeAbsoluteExtended:
            //        // Get the DOS alias for this path
            //        string root = path.Substring(0, rootLength);
            //        string deviceAlias = fileService.QueryDosDeviceNames(root).FirstOrDefault();
            //        if (deviceAlias == null) return null;

            //        // We have to walk drives
            //        foreach (var drive in fileService.GetLogicalDriveStrings())
            //        {
            //            string driveAlias = fileService.QueryDosDeviceNames(drive).FirstOrDefault();
            //            if (String.Equals(deviceAlias, driveAlias))
            //            {
            //                return driveAlias;
            //            }
            //        }
            //        break;
            //}

            //return null;
        }
    }
}
