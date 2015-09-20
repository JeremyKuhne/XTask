// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Utility;
    using XTask.Systems.File;

    internal static partial class NativeMethods
    {
        internal static class VolumeManagement
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365461.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint QueryDosDeviceW(
                    string lpDeviceName,
                    IntPtr lpTargetPath,
                    uint ucchMax);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364975.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetLogicalDriveStringsW(
                    uint nBufferLength,
                    IntPtr lpBuffer);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364996.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetVolumePathNameW(
                    string lpszFileName,
                    StringBuilder lpszVolumePathName,
                    uint cchBufferLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364998.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetVolumePathNamesForVolumeNameW(
                    string lpszVolumeName,
                    IntPtr lpszVolumePathNames,
                    uint cchBuferLength,
                    ref uint lpcchReturnLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364994.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetVolumeNameForVolumeMountPointW(
                    string lpszVolumeMountPoint,
                    StringBuilder lpszVolumeName,
                    uint cchBufferLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364993.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern bool GetVolumeInformationW(
                   string lpRootPathName,
                   StringBuilder lpVolumeNameBuffer,
                   int nVolumeNameSize,
                   out uint lpVolumeSerialNumber,
                   out uint lpMaximumComponentLength,
                   out FileSystemFeature lpFileSystemFlags,
                   StringBuilder lpFileSystemNameBuffer,
                   int nFileSystemNameSize);
            }

            public static IEnumerable<string> QueryDosDevice(string deviceName)
            {
                if (deviceName != null) deviceName = Paths.RemoveTrailingSeparators(deviceName);

                // Null will return everything defined- this list is quite large so set a higher initial allocation
                using (NativeBuffer buffer = new NativeBuffer(deviceName == null ? (uint)8192 : 256))
                {
                    uint result = 0;

                    // QueryDosDevicePrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while ((result = Private.QueryDosDeviceW(deviceName, buffer, (buffer.Size / 2) - 1)) == 0)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        switch (lastError)
                        {
                            case WinError.ERROR_INSUFFICIENT_BUFFER:
                                buffer.Resize(buffer.Size * 2);
                                break;
                            default:
                                throw GetIoExceptionForError(lastError, deviceName);
                        }
                    }

                    return Strings.Split(buffer, (int)result - 2, '\0');
                }
            }

            internal static IEnumerable<string> GetLogicalDriveStrings()
            {
                using (NativeBuffer buffer = new NativeBuffer())
                {
                    uint result = 0;

                    // GetLogicalDriveStringsPrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while ((result = Private.GetLogicalDriveStringsW((uint)buffer.Size / 2, buffer)) > buffer.Size / 2)
                    {
                        buffer.Resize(result * 2);
                    }

                    if (result == 0)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(lastError);
                    }

                    return Strings.Split(buffer, (int)result - 1, '\0');
                }
            }

            internal static string GetVolumePathName(string path)
            {
                // Most paths are mounted at the root, 50 should handle the canonical (guid) root
                StringBuilder volumePathName = new StringBuilder(50);

                while (!Private.GetVolumePathNameW(path, volumePathName, (uint)volumePathName.Capacity))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    switch (lastError)
                    {
                        case WinError.ERROR_FILENAME_EXCED_RANGE:
                            volumePathName.EnsureCapacity(volumePathName.Capacity * 2);
                            break;
                        default:
                            throw GetIoExceptionForError(lastError, path);
                    }
                }

                return volumePathName.ToString();
            }

            internal static IEnumerable<string> GetVolumePathNamesForVolumeName(string volumeName)
            {
                using (NativeBuffer buffer = new NativeBuffer())
                {
                    uint returnLength = 0;

                    // GetLogicalDriveStringsPrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while (!Private.GetVolumePathNamesForVolumeNameW(volumeName, buffer, (uint)buffer.Size / 2, ref returnLength))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        switch (lastError)
                        {
                            case WinError.ERROR_MORE_DATA:
                                buffer.Resize(returnLength * 2);
                                break;
                            default:
                                throw GetIoExceptionForError(lastError, volumeName);
                        }
                    }

                    return Strings.Split(buffer, (int)returnLength - 2, '\0');
                }
            }

            internal static string GetVolumeNameForVolumeMountPoint(string volumeMountPoint)
            {
                volumeMountPoint = Paths.AddTrailingSeparator(volumeMountPoint);

                // MSDN claims 50 is "reasonable", let's go double.
                StringBuilder volumeName = new StringBuilder(100);
                if (!Private.GetVolumeNameForVolumeMountPointW(volumeMountPoint, volumeName, (uint)volumeName.Capacity))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(lastError, volumeMountPoint);
                }

                return volumeName.ToString();
            }

            internal static VolumeInformation GetVolumeInformation(string rootPath)
            {
                rootPath = Paths.AddTrailingSeparator(rootPath);

                StringBuilder volumeName = new StringBuilder(Paths.MaxPath + 1);
                StringBuilder fileSystemName = new StringBuilder(Paths.MaxPath + 1);
                uint serialNumber, maxComponentLength;
                FileSystemFeature flags;
                if (!Private.GetVolumeInformationW(rootPath, volumeName, volumeName.Capacity, out serialNumber, out maxComponentLength, out flags, fileSystemName, fileSystemName.Capacity))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(lastError, rootPath);
                }

                VolumeInformation info = new VolumeInformation
                {
                    RootPathName = rootPath,
                    VolumeName = volumeName.ToString(),
                    VolumeSerialNumber = serialNumber,
                    MaximumComponentLength = maxComponentLength,
                    FileSystemFlags = flags,
                    FileSystemName = fileSystemName.ToString()
                };

                return info;
            }
        }
    }
}
