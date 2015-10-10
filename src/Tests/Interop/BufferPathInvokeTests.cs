// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using XTask.Interop;
    using Xunit;
    using Systems.File;

    public class BufferPathInvokeTests
    {
        [Fact]
        public void NullPathReturnsNull()
        {
            NativeMethods.BufferPathInvoke(null, null).Should().BeNull();
        }

        [Fact]
        public void GetBackOriginalString()
        {
            string input = @"input";
            string output = NativeMethods.BufferPathInvoke(input, (s, b) => { b.Append(s); return (uint)b.Length; });
            input.Should().BeSameAs(output);
        }

        [Fact]
        public void GetBackOriginalLongPathString()
        {
            string input = new string('A', count: Paths.MaxPath + 1);
            string output = NativeMethods.BufferPathInvoke(input, (s, b) => { b.Append(s); return (uint)b.Length; });
            input.Should().BeSameAs(output);
        }

        [Fact]
        public void GetBackOriginalLongUncString()
        {
            string input = new string('\\', count: Paths.MaxPath + 1);
            string output = NativeMethods.BufferPathInvoke(input, (s, b) => { b.Append(s); return (uint)b.Length; });
            input.Should().BeSameAs(output);
        }
    }
}
