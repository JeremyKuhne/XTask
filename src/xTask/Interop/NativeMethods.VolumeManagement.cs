// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Utility;
    using XTask.FileSystem;

    internal static partial class NativeMethods
    {
        [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
        internal static class VolumeManagement
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365461.aspx
            [DllImport("kernel32.dll", EntryPoint = "QueryDosDeviceW", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern uint QueryDosDevicePrivate(string lpDeviceName, IntPtr lpTargetPath, int ucchMax);

            public static IEnumerable<string> QueryDosDevice(string deviceName)
            {
                if (deviceName != null) deviceName = Paths.RemoveTrailingSeparators(deviceName);

                // Null will return everything defined- this list is quite large so set a higher initial allocation
                using (NativeBuffer buffer = new NativeBuffer(deviceName == null ? 8192 : 256))
                {
                    uint result = 0;

                    // QueryDosDevicePrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while ((result = QueryDosDevicePrivate(deviceName, buffer, buffer.Size / 2)) == 0)
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

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364975.aspx
            [DllImport("kernel32.dll", EntryPoint = "GetLogicalDriveStringsW", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern uint GetLogicalDriveStringsPrivate(uint nBufferLength, IntPtr lpBuffer);

            internal static IEnumerable<string> GetLogicalDriveStrings()
            {
                using (NativeBuffer buffer = new NativeBuffer())
                {
                    uint result = 0;

                    // GetLogicalDriveStringsPrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while ((result = GetLogicalDriveStringsPrivate((uint)buffer.Size / 2, buffer)) > buffer.Size / 2)
                    {
                        buffer.Resize((int)result * 2);
                    }

                    if (result == 0)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(lastError);
                    }

                    return Strings.Split(buffer, (int)result - 1, '\0');
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364996.aspx
            [DllImport("kernel32.dll", EntryPoint = "GetVolumePathNameW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetVolumePathNamePrivate(
                string lpszFileName,
                StringBuilder lpszVolumePathName,
                uint cchBufferLength);

            internal static string GetVolumePathName(string fileName)
            {
                StringBuilder volumePathName = new StringBuilder(10);

                while (!GetVolumePathNamePrivate(fileName, volumePathName, (uint)volumePathName.Capacity))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    switch (lastError)
                    {
                        case WinError.ERROR_FILENAME_EXCED_RANGE:
                            volumePathName.EnsureCapacity(volumePathName.Length * 2);
                            break;
                        default:
                            throw GetIoExceptionForError(lastError, fileName);
                    }
                }

                return volumePathName.ToString();
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364998.aspx
            [DllImport("kernel32.dll", EntryPoint = "GetVolumePathNamesForVolumeNameW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetVolumePathNamesForVolumeNamePrivate(
                string lpszVolumeName,
                IntPtr lpszVolumePathNames,
                uint cchBuferLength,
                ref uint lpcchReturnLength);

            internal static IEnumerable<string> GetVolumePathNamesForVolumeName(string volumeName)
            {
                using (NativeBuffer buffer = new NativeBuffer())
                {
                    uint returnLength = 0;

                    // GetLogicalDriveStringsPrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while (!GetVolumePathNamesForVolumeNamePrivate(volumeName, buffer, (uint)buffer.Size / 2, ref returnLength))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        switch (lastError)
                        {
                            case WinError.ERROR_MORE_DATA:
                                buffer.Resize((int)returnLength * 2);
                                break;
                            default:
                                throw GetIoExceptionForError(lastError, volumeName);
                        }
                    }

                    return Strings.Split(buffer, (int)returnLength - 2, '\0');
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364994.aspx
            [DllImport("kernel32.dll", EntryPoint = "GetVolumeNameForVolumeMountPoint", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetVolumeNameForVolumeMountPointPrivate(
                string lpszVolumeMountPoint,
                StringBuilder lpszVolumeName,
                uint cchBufferLength);

            internal static string GetVolumeNameForVolumeMountPoint(string volumeMountPoint)
            {
                volumeMountPoint = Paths.AddTrailingSeparator(volumeMountPoint);

                // MSDN claims 50 is "reasonable", let's go double.
                StringBuilder volumeName = new StringBuilder(100);
                if (!GetVolumeNameForVolumeMountPointPrivate(volumeMountPoint, volumeName, (uint)volumeName.Capacity))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(lastError, volumeMountPoint);
                }

                return volumeName.ToString();
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364993.aspx
            [DllImport("Kernel32.dll", EntryPoint = "GetVolumeInformation", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern bool GetVolumeInformationPrivate(
               string lpRootPathName,
               StringBuilder lpVolumeNameBuffer,
               int nVolumeNameSize,
               out uint lpVolumeSerialNumber,
               out uint lpMaximumComponentLength,
               out FileSystemFeature lpFileSystemFlags,
               StringBuilder lpFileSystemNameBuffer,
               int nFileSystemNameSize);

            internal static VolumeInformation GetVolumeInformation(string rootPath)
            {
                rootPath = Paths.AddTrailingSeparator(rootPath);

                StringBuilder volumeName = new StringBuilder(Paths.MaxPath + 1);
                StringBuilder fileSystemName = new StringBuilder(Paths.MaxPath + 1);
                uint serialNumber, maxComponentLength;
                FileSystemFeature flags;
                if (!GetVolumeInformationPrivate(rootPath, volumeName, volumeName.Capacity, out serialNumber, out maxComponentLength, out flags, fileSystemName, fileSystemName.Capacity))
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
