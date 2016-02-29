// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static partial class NativeMethods
    {
        // Technically WinErrors are uints (GetLastError returns DWORD). .NET uses int for these errors so we will as well
        // to facilitate interaction with Marshal.GetLastWin32Error(), etc.

        private static class Errors
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            internal static class Private
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms721800.aspx
                [DllImport(Libraries.Advapi32, SetLastError = true, ExactSpelling = true)]
                internal static extern int LsaNtStatusToWinError(int Status);
            }

            // [MS-ERREF] NTSTATUS
            // https://msdn.microsoft.com/en-us/library/cc231200.aspx
            private const int STATUS_SEVERITY_SUCCESS = 0x0;
            private const int STATUS_SEVERITY_INFORMATIONAL = 0x1;
            private const int STATUS_SEVERITY_WARNING = 0x2;
            private const int STATUS_SEVERITY_ERROR = 0x3;

            internal static bool NT_SUCCESS(int NTSTATUS)
            {
                return NTSTATUS >= 0;
            }

            internal static bool NT_INFORMATION(int NTSTATUS)
            {
                return (uint)NTSTATUS >> 30 == STATUS_SEVERITY_INFORMATIONAL;
            }

            internal static bool NT_WARNING(int NTSTATUS)
            {
                return (uint)NTSTATUS >> 30 == STATUS_SEVERITY_WARNING;
            }

            internal static bool NT_ERROR(int NTSTATUS)
            {
                return (uint)NTSTATUS >> 30 == STATUS_SEVERITY_ERROR;
            }

            internal static int NtStatusToWinError(int status)
            {
                return Private.LsaNtStatusToWinError(status);
            }
        }

        internal static class WinError
        {
            // From winerror.h
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382.aspx
            internal const int NO_ERROR = 0;
            internal const int ERROR_SUCCESS = 0;
            internal const int ERROR_INVALID_FUNCTION = 1;
            internal const int ERROR_FILE_NOT_FOUND = 2;
            internal const int ERROR_PATH_NOT_FOUND = 3;
            internal const int ERROR_ACCESS_DENIED = 5;
            internal const int ERROR_INVALID_DRIVE = 15;
            internal const int ERROR_NO_MORE_FILES = 18;
            internal const int ERROR_NOT_READY = 21;
            internal const int ERROR_SEEK = 25;
            internal const int ERROR_SHARING_VIOLATION = 32;
            internal const int ERROR_BAD_NETPATH = 53;
            internal const int ERROR_NETNAME_DELETED = 64;
            internal const int ERROR_NETWORK_ACCESS_DENIED = 65;
            internal const int ERROR_BAD_NET_NAME = 67;
            internal const int ERROR_FILE_EXISTS = 80;
            internal const int ERROR_INVALID_PARAMETER = 87;
            internal const int ERROR_INSUFFICIENT_BUFFER = 122;
            internal const int ERROR_INVALID_NAME = 123;
            internal const int ERROR_BAD_PATHNAME = 161;
            internal const int ERROR_ALREADY_EXISTS = 183;
            internal const int ERROR_ENVVAR_NOT_FOUND = 203;
            internal const int ERROR_FILENAME_EXCED_RANGE = 206;
            internal const int ERROR_MORE_DATA = 234;
            internal const int ERROR_OPERATION_ABORTED = 995;
            internal const int ERROR_NO_TOKEN = 1008;
            internal const int ERROR_NOT_FOUND = 1168;
            internal const int ERROR_PRIVILEGE_NOT_HELD = 1314;
            internal const int ERROR_DISK_CORRUPT = 1393;
        }

        internal static class NtStatus
        {
            // NTSTATUS values
            // https://msdn.microsoft.com/en-us/library/cc704588.aspx
            internal const int STATUS_SUCCESS = 0x00000000;

            /// <summary>
            /// {Buffer Overflow} The data was too large to fit into the specified buffer.
            /// </summary>
            internal const int STATUS_BUFFER_OVERFLOW = unchecked((int)0x80000005);

            /// <summary>
            /// The specified information record length does not match the length that is required for the specified information class.
            /// </summary>
            internal const int STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004);

            /// <summary>
            /// An invalid HANDLE was specified.
            /// </summary>
            internal const int STATUS_INVALID_HANDLE = unchecked((int)0xC0000008);

            /// <summary>
            /// An invalid parameter was passed to a service or function.
            /// </summary>
            internal const int STATUS_INVALID_PARAMETER = unchecked((int)0xC000000D);

            /// <summary>
            /// {Access Denied} A process has requested access to an object but has not been granted those access rights.
            /// </summary>
            internal const int STATUS_ACCESS_DENIED = unchecked((int)0xC0000022);

            /// <summary>
            /// {Buffer Too Small} The buffer is too small to contain the entry. No information has been written to the buffer.
            /// </summary>
            internal const int STATUS_BUFFER_TOO_SMALL = unchecked((int)0xC0000023);

        }

        internal static Exception GetIoExceptionForError(int error, string path = null)
        {
            // http://referencesource.microsoft.com/#mscorlib/system/io/__error.cs,142

            string errorText = $"{NativeErrorHelper.LastErrorToString(error)} : '{path ?? XTaskStrings.NoValue}'";

            switch (error)
            {
                case WinError.ERROR_FILE_NOT_FOUND:
                    return new FileNotFoundException(errorText, path);
                case WinError.ERROR_PATH_NOT_FOUND:
                    return new DirectoryNotFoundException(errorText);
                case WinError.ERROR_ACCESS_DENIED:
                // Network access doesn't throw UnauthorizedAccess in .NET
                case WinError.ERROR_NETWORK_ACCESS_DENIED:
                    return new UnauthorizedAccessException(errorText);
                case WinError.ERROR_FILENAME_EXCED_RANGE:
                    return new PathTooLongException(errorText);
                case WinError.ERROR_INVALID_DRIVE:
                    return new DriveNotFoundException(errorText);
                case WinError.ERROR_OPERATION_ABORTED:
                    return new OperationCanceledException(errorText);
                case WinError.ERROR_ALREADY_EXISTS:
                case WinError.ERROR_SHARING_VIOLATION:
                case WinError.ERROR_FILE_EXISTS:
                default:
                    return new IOException(errorText, NativeErrorHelper.GetHResultForWindowsError(error));
            }
        }
    }
}
