// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.IO;

    internal static partial class NativeMethods
    {
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
