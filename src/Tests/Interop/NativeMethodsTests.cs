// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices.ComTypes;
    using XTask.Interop;
    using Xunit;
    using FluentAssertions;

    public class NativeMethodsTests
    {
        [Theory
            InlineData(0, 0, 0x0701cdd41453c000)    // January 1, 1601
            InlineData(0, 1, 0x0701cdd41453c001)
            InlineData(0, -1, 0x0701cdd51453bfff)   // If we don't cast to uint this would throw an exception
            ]
        public void GetDateTimeTests(int high, int low, long expectedTicks)
        {
            FILETIME fileTime;
            fileTime.dwHighDateTime = high;
            fileTime.dwLowDateTime = low;

            DateTime dt = NativeMethods.GetDateTime(fileTime);
            dt.Ticks.Should().Be(expectedTicks);
        }

        [Fact]
        public void GetPipeObjectInfo()
        {
            var fileHandle = NativeMethods.FileManagement.CreateFile(
                @"\\.\pipe\",
                0,                  // We don't care about read or write, we're just getting metadata with this handle
                System.IO.FileShare.ReadWrite,
                System.IO.FileMode.Open,
                NativeMethods.FileManagement.AllFileAttributeFlags.FILE_ATTRIBUTE_NORMAL
                    | NativeMethods.FileManagement.AllFileAttributeFlags.FILE_FLAG_OPEN_REPARSE_POINT   // To avoid traversing links
                    | NativeMethods.FileManagement.AllFileAttributeFlags.FILE_FLAG_BACKUP_SEMANTICS);   // To open directories

            string name = NativeMethods.Handles.GetObjectName(fileHandle.DangerousGetHandle());
            name.Should().Be(@"\Device\NamedPipe\");

            string typeName = NativeMethods.Handles.GetObjectTypeName(fileHandle.DangerousGetHandle());
            typeName.Should().Be(@"File");
        }
    }
}
