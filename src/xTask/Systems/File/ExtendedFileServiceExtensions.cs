﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File;

public static class ExtendedFileServiceExtensions
{
    /// <summary>
    ///  Get the canonical volume root (e.g. the \\?\VolumeGuid format) for the given path. The path must not be relative.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the path is relative or indeterminate.</exception>
    /// <exception cref="System.IO.IOException">Thrown if unable to get the root from the OS.</exception>
    public static string GetCanonicalRoot(this IExtendedFileService extendedFileService, IFileService fileService, string path)
    {
        if (Paths.IsPartiallyQualified(path)) throw new ArgumentException(nameof(path));

        path = fileService.GetFullPath(path);
        var format = Paths.GetPathFormat(path, out int rootLength);
        if (format == PathFormat.UnknownFormat) throw new ArgumentException(nameof(format));

        string root = path.Substring(0, rootLength);
        string canonicalRoot = root;

        switch (format)
        {
            case PathFormat.UniformNamingConvention:
                if (Paths.IsDevice(path)) canonicalRoot = @"\\" + root.Substring(Paths.ExtendedUncPrefix.Length);
                break;
            case PathFormat.LocalFullyQualified:
                canonicalRoot = extendedFileService.GetVolumeName(root);
                break;
        }

        return canonicalRoot;
    }

    /// <summary>
    ///  Return the legacy drive letter (e.g. "C", "D") for the given path or null if one doesn't exist
    /// </summary>
    public static string GetDriveLetter(this IExtendedFileService extendedFileService, IFileService fileService, string path)
    {
        string pathCanonicalRoot = extendedFileService.GetCanonicalRoot(fileService, path);

        // We have to walk drives
        foreach (string drive in extendedFileService.GetLogicalDriveStrings())
        {
            string driveCanonicalRoot = extendedFileService.GetCanonicalRoot(fileService, drive);
            if (string.Equals(pathCanonicalRoot, driveCanonicalRoot))
            {
                return drive;
            }
        }

        return null;
    }
}
