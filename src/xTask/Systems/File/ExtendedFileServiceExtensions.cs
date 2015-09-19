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
    }
}
