// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using XTask.Systems.File;
    using Utility;

    internal static partial class NativeMethods
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363950.aspx
        internal static class DirectoryManagement
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365488.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool RemoveDirectoryW(
                    string lpPathName);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363855.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CreateDirectoryW(
                    string lpPathName,
                    IntPtr lpSecurityAttributes);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364934.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetCurrentDirectoryW(
                    uint nBufferLength,
                    StringBuilder lpBuffer);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365530.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool SetCurrentDirectoryW(
                    string lpPathName);
            }

            internal static void RemoveDirectory(string path)
            {
                path = Paths.AddExtendedPrefix(NativeMethods.FileManagement.GetFullPathName(path));

                if (!Private.RemoveDirectoryW(path))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }
            }

            internal static void CreateDirectory(string path)
            {
                // CreateDirectory will refuse paths that are over MAX_PATH - 12, so we always want to add the prefix
                path = Paths.AddExtendedPrefix(path, addIfUnderLegacyMaxPath: true);

                if (!Private.CreateDirectoryW(path, IntPtr.Zero))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }
            }

            internal static string GetCurrentDirectory()
            {
                // Call to get the needed size
                uint result = Private.GetCurrentDirectoryW(0, null);
                if (result == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }

                var sb = StringBuilderCache.Instance.Acquire();
                sb.EnsureCapacity((int)result);
                result = Private.GetCurrentDirectoryW((uint)sb.Capacity, sb);

                if (result == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    StringBuilderCache.Instance.Release(sb);
                    throw GetIoExceptionForError(error);
                }

                return StringBuilderCache.Instance.ToStringAndRelease(sb);
            }

            internal static void SetCurrentDirectory(string path)
            {
                if (!Private.SetCurrentDirectoryW(path))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }
            }
        }
    }
}
