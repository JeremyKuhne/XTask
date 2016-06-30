// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using WInterop.Authorization;
using WInterop.Backup;
using WInterop.FileManagement;
using WInterop.VolumeManagement;
using WInterop.Authorization.DataTypes;
using WInterop.FileManagement.DataTypes;

namespace XTask.Systems.File.Concrete
{
    /// <summary>
    /// Basic implementation of extended file service support. These methods have no .NET implementation.
    /// </summary>
    public class ExtendedFileService : IExtendedFileService
    {
        public string GetFinalPath(string path, bool resolveLinks = false)
        {
            return FileDesktopMethods.GetFinalPathName(Paths.AddExtendedPrefix(path), GetFinalPathNameByHandleFlags.FILE_NAME_NORMALIZED, resolveLinks);
        }

        public string GetLongPath(string path)
        {
            return FileDesktopMethods.GetLongPathName(Paths.AddExtendedPrefix(path));
        }

        public string GetShortPath(string path)
        {
            return FileDesktopMethods.GetShortPathName(Paths.AddExtendedPrefix(path));
        }

        public string GetVolumeName(string volumeMountPoint)
        {
            if (string.IsNullOrWhiteSpace(volumeMountPoint)) throw new ArgumentNullException(nameof(volumeMountPoint));

            return VolumeDesktopMethods.GetVolumeNameForVolumeMountPoint(volumeMountPoint);
        }

        public string GetMountPoint(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            return VolumeDesktopMethods.GetVolumePathName(path);
        }

        public IEnumerable<string> GetVolumeMountPoints(string volumeName)
        {
            if (string.IsNullOrWhiteSpace(volumeName)) throw new ArgumentNullException(nameof(volumeName));

            return VolumeDesktopMethods.GetVolumePathNamesForVolumeName(volumeName);
        }

        public IEnumerable<string> QueryDosDeviceNames(string dosAlias)
        {
            return VolumeDesktopMethods.QueryDosDevice(dosAlias);
        }

        public IEnumerable<string> GetLogicalDriveStrings()
        {
            return VolumeDesktopMethods.GetLogicalDriveStrings();
        }

        public VolumeInformation GetVolumeInformation(string rootPath)
        {
            var info =  VolumeDesktopMethods.GetVolumeInformation(rootPath);
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
                from info in BackupDesktopMethods.GetAlternateStreamInformation(path)
                select new AlternateStreamInformation
                {
                    Name = info.Name,
                    Size = info.Size
                };
        }

        public bool CanCreateSymbolicLinks()
        {
            // Assuming that the current thread can replicate rights from the process
            using (var processToken = AuthorizationDesktopMethods.OpenProcessToken(TokenRights.TOKEN_QUERY | TokenRights.TOKEN_READ))
            {
                return AuthorizationDesktopMethods.HasPrivilege(processToken, Privileges.SeCreateSymbolicLinkPrivilege);
            }
        }
    }
}
