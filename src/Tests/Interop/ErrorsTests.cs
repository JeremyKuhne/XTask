// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Interop;
    using Xunit;

    public class ErrorsTests
    {
        [Theory
            InlineData(NativeMethods.WinError.ERROR_FILE_NOT_FOUND, typeof(FileNotFoundException))
            InlineData(NativeMethods.WinError.ERROR_PATH_NOT_FOUND, typeof(DirectoryNotFoundException))
            InlineData(NativeMethods.WinError.ERROR_ACCESS_DENIED, typeof(UnauthorizedAccessException))
            InlineData(NativeMethods.WinError.ERROR_NETWORK_ACCESS_DENIED, typeof(UnauthorizedAccessException))
            InlineData(NativeMethods.WinError.ERROR_FILENAME_EXCED_RANGE, typeof(PathTooLongException))
            InlineData(NativeMethods.WinError.ERROR_INVALID_DRIVE, typeof(DriveNotFoundException))
            InlineData(NativeMethods.WinError.ERROR_OPERATION_ABORTED, typeof(OperationCanceledException))
            InlineData(NativeMethods.WinError.ERROR_ALREADY_EXISTS, typeof(IOException))
            InlineData(NativeMethods.WinError.ERROR_SHARING_VIOLATION, typeof(IOException))
            InlineData(NativeMethods.WinError.ERROR_FILE_EXISTS, typeof(IOException))
            ]
        public void ErrorsMapToExceptions(int error, Type exceptionType)
        {
            NativeMethods.GetIoExceptionForError(error).Should().BeOfType(exceptionType);
        }
    }
}
