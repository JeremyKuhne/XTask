// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using System;
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
    }
}
