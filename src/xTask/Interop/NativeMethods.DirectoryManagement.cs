// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using XTask.FileSystem;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Utility;

    internal static partial class NativeMethods
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363950.aspx
        [SuppressUnmanagedCodeSecurity]
        internal static class DirectoryManagement
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365488.aspx
            [DllImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool RemoveDirectoryPrivate(
                string lpPathName);

            internal static void RemoveDirectory(string path)
            {
                path = Paths.AddExtendedPathPrefix(NativeMethods.FileManagement.GetFullPathName(path));

                if (!RemoveDirectoryPrivate(path))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363855.aspx
            [DllImport("kernel32.dll", EntryPoint = "CreateDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CreateDirectoryPrivate(
                string lpPathName,
                IntPtr lpSecurityAttributes);

            internal static void CreateDirectory(string path)
            {
                // CreateDirectory will refuse paths that are over MAX_PATH - 12, so we always want to add the prefix
                path = Paths.AddExtendedPathPrefix(path, addIfUnderLegacyMaxPath: true);

                if (!CreateDirectoryPrivate(path, IntPtr.Zero))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364934.aspx
            [DllImport("kernel32.dll", EntryPoint = "GetCurrentDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern uint GetCurrentDirectoryPrivate(
                uint nBufferLength,
                StringBuilder lpBuffer
                );

            internal static string GetCurrentDirectory()
            {
                // Call to get the needed size
                uint result = GetCurrentDirectoryPrivate(0, null);
                if (result == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }

                var sb = NativeMethods.stringCache.Acquire();
                sb.EnsureCapacity((int)result);
                result = GetCurrentDirectoryPrivate((uint)sb.Capacity, sb);

                if (result == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    NativeMethods.stringCache.Release(sb);
                    throw GetIoExceptionForError(error);
                }

                return NativeMethods.stringCache.ToStringAndRelease(sb);
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365530.aspx
            [DllImport("kernel32.dll", EntryPoint = "SetCurrentDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetCurrentDirectoryPrivate(
                string lpPathName
                );

            internal static void SetCurrentDirectory(string path)
            {
                if (!SetCurrentDirectoryPrivate(path))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }
            }
        }
    }
}
