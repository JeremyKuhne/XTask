// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using WInterop.Storage;
using WInterop.Security;

namespace XTask.Systems.File.Concrete
{
    /// <summary>
    /// Basic implementation of extended file service support. These methods have no .NET implementation.
    /// </summary>
    public class ExtendedFileService : IExtendedFileService
    {
        public string GetFinalPath(string path, bool resolveLinks = false)
        {
            return Storage.GetFinalPathName(Paths.AddExtendedPrefix(path), GetFinalPathNameByHandleFlags.FileNameNormalized, resolveLinks);
        }

        public string GetLongPath(string path)
        {
            return Storage.GetLongPathName(Paths.AddExtendedPrefix(path));
        }

        public string GetShortPath(string path)
        {
            return Storage.GetShortPathName(Paths.AddExtendedPrefix(path));
        }

        public string GetVolumeName(string volumeMountPoint)
        {
            if (string.IsNullOrWhiteSpace(volumeMountPoint)) throw new ArgumentNullException(nameof(volumeMountPoint));

            return Storage.GetVolumeNameForVolumeMountPoint(volumeMountPoint);
        }

        public string GetMountPoint(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            return Storage.GetVolumePathName(path);
        }

        public IEnumerable<string> GetVolumeMountPoints(string volumeName)
        {
            if (string.IsNullOrWhiteSpace(volumeName)) throw new ArgumentNullException(nameof(volumeName));

            return Storage.GetVolumePathNamesForVolumeName(volumeName);
        }

        public IEnumerable<string> QueryDosDeviceNames(string dosAlias)
        {
            return Storage.QueryDosDevice(dosAlias);
        }

        public IEnumerable<string> GetLogicalDriveStrings()
        {
            return Storage.GetLogicalDriveStrings();
        }

        public VolumeInformation GetVolumeInformation(string rootPath)
        {
            var info =  Storage.GetVolumeInformation(rootPath);
            return new VolumeInformation
            {
                FileSystemFlags = (FileSystemFeature)info.FileSystemFlags,
                FileSystemName = info.FileSystemName,
                RootPathName = info.RootPathName,
                MaximumComponentLength = info.MaximumComponentLength,
                VolumeName = info.VolumeName,
                VolumeSerialNumber = info.VolumeSerialNumber
            };
        }

        public IEnumerable<AlternateStreamInformation> GetAlternateStreamInformation(string path)
        {
            return
                from info in Storage.GetAlternateStreamInformation(path)
                select new AlternateStreamInformation
                {
                    Name = info.Name,
                    Size = (ulong)info.Size
                };
        }

        public bool CanCreateSymbolicLinks()
        {
            // Assuming that the current thread can replicate rights from the process
            using (var processToken = Security.OpenProcessToken(AccessTokenRights.Query | AccessTokenRights.Read))
            {
                return Security.HasPrivilege(processToken, Privilege.CreateSymbolicLink);
            }
        }
    }
}
