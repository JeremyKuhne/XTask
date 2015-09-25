// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Utility
{
    using FluentAssertions;
    using XTask.Interop;
    using Xunit;

    public class ErrorHelperTests
    {
        [Theory,
            InlineData(0, @"Error 0: The operation completed successfully"),
            InlineData(2, @"Error 2: The system cannot find the file specified"),
            InlineData(3, @"Error 3: The system cannot find the path specified"),
            InlineData(123, @"Error 123: The filename, directory name, or volume label syntax is incorrect")]
        public void WindowsErrorTextIsAsExpected(int error, string expected)
        {
            NativeErrorHelper.LastErrorToString(error).Should().Be(expected);
        }
    }
}
