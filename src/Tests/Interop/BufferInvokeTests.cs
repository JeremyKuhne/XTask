// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using XTask.Interop;
    using Xunit;

    public class BufferInvokeTests
    {
        [Fact]
        public void NoHandlerShouldThrow()
        {
            Action action = () =>
                NativeMethods.BufferInvoke(sb => 0);
            action.ShouldThrow<Exception>();
        }

        [Fact]
        public void HandlerShouldNotThrow()
        {
            NativeMethods.BufferInvoke(sb => 0, "Test", error => false).Should().BeNull();
        }
    }
}
