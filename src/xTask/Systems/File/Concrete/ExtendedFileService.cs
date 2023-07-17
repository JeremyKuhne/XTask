// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Support;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using xTask.Utility;
using XTask.Systems.File.Concrete.Flex;

namespace XTask.Systems.File.Concrete
{
    /// <summary>
    ///  Basic implementation of extended file service support. These methods have no .NET implementation.
    /// </summary>
    public unsafe class ExtendedFileService : IExtendedFileService
    {
        private static LUID? s_symbolicLinkLuid;

        public string GetFinalPath(string path, bool resolveLinks = false)
        {
            // BackupSemantics is needed to get directory handles

            FILE_FLAGS_AND_ATTRIBUTES flags = FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS;
            if (resolveLinks)
            {
                flags |= FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OPEN_REPARSE_POINT;
            }

            using SafeFileHandle fileHandle = Storage.CreateFile(
                path,
                default,
                System.IO.FileShare.ReadWrite,
                System.IO.FileMode.Open,
                default,
                flags);

            using BufferScope<char> buffer = new(stackalloc char[(int)Interop.MAX_PATH]);

            while (true)
            {
                fixed (char* b = buffer)
                {
                    uint count = Interop.GetFinalPathNameByHandle(
                        (HANDLE)fileHandle.DangerousGetHandle(),
                        b,
                        (uint)buffer.Length,
                        GETFINALPATHNAMEBYHANDLE_FLAGS.FILE_NAME_NORMALIZED);

                    if (count == 0)
                    {
                        Error.ThrowLastError(path);
                    }

                    if (count <= buffer.Length)
                    {
                        return buffer.Slice(0, (int)count).ToString();
                    }

                    buffer.EnsureCapacity((int)count);
                }
            }
        }

        public string GetLongPath(string path)
        {
            string extendedPath = Paths.AddExtendedPrefix(path);
            using BufferScope<char> buffer = new(stackalloc char[260]);
            while (true)
            {
                fixed (char* b = buffer)
                {
                    uint count = Interop.GetLongPathName(extendedPath, b, (uint)buffer.Length);
                    if (count == 0)
                    {
                        Error.ThrowLastError(path);
                    }

                    if (count <= buffer.Length)
                    {
                        return buffer.ToString(count, extendedPath);
                    }

                    buffer.EnsureCapacity((int)count);
                }
            }
        }

        public string GetShortPath(string path)
        {
            string extendedPath = Paths.AddExtendedPrefix(path);
            using BufferScope<char> buffer = new(stackalloc char[260]);
            while (true)
            {
                fixed (char* b = buffer)
                {
                    uint count = Interop.GetShortPathName(extendedPath, b, (uint)buffer.Length);
                    if (count == 0)
                    {
                        Error.ThrowLastError(path);
                    }

                    if (count <= buffer.Length)
                    {
                        return buffer.ToString(count, extendedPath);
                    }

                    buffer.EnsureCapacity((int)count);
                }
            }
        }

        public string GetVolumeName(string volumeMountPoint)
        {
            if (string.IsNullOrWhiteSpace(volumeMountPoint)) throw new ArgumentNullException(nameof(volumeMountPoint));

            volumeMountPoint = Paths.EnsureTrailingSeparator(volumeMountPoint);

            Span<char> buffer = stackalloc char[50];
            fixed (char* b = buffer)
            {
                Interop.GetVolumeNameForVolumeMountPoint(volumeMountPoint, b, (uint)buffer.Length).ThrowLastErrorIfFalse();
            }

            return buffer.SliceAtNull().ToString();
        }

        public string GetMountPoint(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            using BufferScope<char> buffer = new(stackalloc char[260]);
            while (true)
            {
                fixed (char* b = buffer)
                {
                    if (Interop.GetVolumePathName(path, b, (uint)buffer.Length))
                    {
                        return buffer.ToStringAtNull();
                    }

                    Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE);
                    buffer.IncreaseCapacity(1024);
                }
            }
        }

        public IEnumerable<string> GetVolumeMountPoints(string volumeName)
        {
            if (string.IsNullOrWhiteSpace(volumeName)) throw new ArgumentNullException(nameof(volumeName));

            using BufferScope<char> buffer = new(stackalloc char[512]);
            while (true)
            {
                fixed (char* b = buffer)
                {
                    if (Interop.GetVolumePathNamesForVolumeName(volumeName, b, (uint)buffer.Length, out uint count))
                    {
                        // If the return length is 1 there were no mount points. The buffer should be '\0'.
                        if (count < 3)
                        {
                            Debug.Assert(buffer[0] == '\0');
                            return Enumerable.Empty<string>();
                        }

                        // The return length will be the entire length of the buffer, including the final string's
                        // null and the string list's second null. Example: "Foo\0Bar\0\0" would be 9.
                        return buffer.Slice(0, (int)count - 2).Split('\0');
                    }

                    Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_MORE_DATA, volumeName);
                    buffer.EnsureCapacity((int)count);
                }
            }
        }

        public IEnumerable<string> QueryDosDeviceNames(string dosAlias)
        {
            if (dosAlias is not null)
            {
                dosAlias = Paths.RemoveTrailingSeparators(dosAlias);
            }

            // Null will return everything defined- this list is quite large so set a higher initial allocation

            using BufferScope<char> buffer = new(dosAlias is null ? 16384 : 1024);
            uint count = 0;

            while (true)
            {
                fixed (char* b = buffer)
                {
                    count = Interop.QueryDosDevice(dosAlias, b, (uint)buffer.Length);

                    if (count <= buffer.Length)
                    {
                        // Trim out the trailing double nulls.
                        return buffer.Slice(0, (int)count - 2).Split('\0');
                    }

                    Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER);
                    buffer.IncreaseCapacity(4096);
                }
            }
        }

        public IEnumerable<string> GetLogicalDriveStrings()
        {
            using BufferScope<char> buffer = new(1024);
            uint count = 0;

            while (true)
            {
                fixed (char* b = buffer)
                {
                    count = Interop.GetLogicalDriveStrings((uint)buffer.Length, b);
                    if (count == 0)
                    {
                        Error.ThrowLastError();
                    }

                    if (count < buffer.Length)
                    {
                        return buffer.Slice(0, (int)count).Split('\0');
                    }

                    buffer.EnsureCapacity((int)count);
                }
            }
        }

        public VolumeInformation GetVolumeInformation(string rootPath)
        {
            using BufferScope<char> volumeNameBuffer = new(stackalloc char[(int)Interop.MAX_PATH]);
            using BufferScope<char> fileSystemNameBuffer = new(stackalloc char[(int)Interop.MAX_PATH]);

            if (rootPath is not null)
            {
                rootPath = Paths.EnsureTrailingSeparator(rootPath);
            }

            fixed (char* r = rootPath)
            fixed (char* v = volumeNameBuffer)
            fixed (char* f = fileSystemNameBuffer)
            {
                uint serialNumber;
                uint maxComponentLength;
                uint flags;

                Interop.GetVolumeInformation(
                    r,
                    v, (uint)volumeNameBuffer.Length,
                    &serialNumber,
                    &maxComponentLength,
                    &flags,
                    f, (uint)fileSystemNameBuffer.Length).ThrowLastErrorIfFalse(rootPath);

                return new VolumeInformation
                {
                    FileSystemFlags = (FileSystemFeature)flags,
                    FileSystemName = fileSystemNameBuffer.ToStringAtNull(),
                    RootPathName = rootPath,
                    MaximumComponentLength = maxComponentLength,
                    VolumeName = volumeNameBuffer.ToStringAtNull(),
                    VolumeSerialNumber = serialNumber
                };
            }
        }

        public IEnumerable<AlternateStreamInformation> GetAlternateStreamInformation(string path)
        {
            using var fileHandle = Storage.CreateFile(
                path: path,
                // To look at metadata we don't need read or write access
                fileAccess: default,
                shareMode: System.IO.FileShare.ReadWrite,
                fileMode: System.IO.FileMode.Open,
                fileAttributes: default,
                flagsAndAttributes: FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS);

            using BackupReader reader = new(fileHandle);
            List<AlternateStreamInformation> alternateInfo = new();
            while (reader.GetNextInfo())
            {
                if (reader.Current.StreamType == WIN_STREAM_ID.BACKUP_ALTERNATE_DATA)
                {
                    alternateInfo.Add(new() { Name = reader.Current.Name, Size = (ulong)reader.Current.Size });
                }
            }

            return alternateInfo;
        }

        public bool CanCreateSymbolicLinks()
        {
            // Open the process access token.
            HANDLE token;
            Interop.OpenProcessToken(
                Interop.GetCurrentProcess(),
                TOKEN_ACCESS_MASK.TOKEN_QUERY | TOKEN_ACCESS_MASK.TOKEN_READ,
                &token).ThrowLastErrorIfFalse();

            // Lookup the symlink priviledge LUID if we haven't already.
            LUID symlinkPriviledge;
            if (!s_symbolicLinkLuid.HasValue)
            {
                Interop.LookupPrivilegeValue(
                    null,
                    Interop.SE_CREATE_SYMBOLIC_LINK_NAME,
                    out symlinkPriviledge).ThrowLastErrorIfFalse();

                s_symbolicLinkLuid = symlinkPriviledge;
            }
            else
            {
                symlinkPriviledge = s_symbolicLinkLuid.Value;
            }

            try
            {
                // Get the token information
                using BufferScope<byte> buffer = new(stackalloc byte[512]);
                while (true)
                {
                    fixed (byte* b = buffer)
                    {
                        if (Interop.GetTokenInformation(
                            token,
                            TOKEN_INFORMATION_CLASS.TokenPrivileges,
                            b,
                            (uint)buffer.Length,
                            out uint length))
                        {
                            break;
                        }

                        Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER);
                        buffer.EnsureCapacity((int)length);
                    }
                }

                // Look to see if it has the priviledge we're looking for
                fixed (void* b = buffer)
                {
                    TOKEN_PRIVILEGES* tp = (TOKEN_PRIVILEGES*)b;
                    ReadOnlySpan<LUID_AND_ATTRIBUTES> identifiers = new(&tp->Privileges, (int)tp->PrivilegeCount);
                    foreach (LUID_AND_ATTRIBUTES identifier in identifiers)
                    {
                        if (identifier.Luid.HighPart == symlinkPriviledge.HighPart
                            && identifier.Luid.LowPart == symlinkPriviledge.LowPart)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            finally
            {
                Interop.CloseHandle(token);
            }
        }

    }
}
