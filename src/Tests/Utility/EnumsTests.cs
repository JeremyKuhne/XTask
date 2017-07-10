// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Utility
{
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Utility;
    using Xunit;

    public class EnumsTests
    {
        [Theory,
            InlineData(FileAttributes.ReadOnly, new FileAttributes[] { FileAttributes.ReadOnly }),
            InlineData(FileAttributes.ReadOnly | FileAttributes.Hidden, new FileAttributes[] { FileAttributes.ReadOnly, FileAttributes.Hidden })
            ]
        public void TestGetSetValues(FileAttributes value, FileAttributes[] expected)
        {
            Enums.GetSetValues(value).ShouldBeEquivalentTo(expected);
        }
    }
}
