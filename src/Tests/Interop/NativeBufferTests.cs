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

    public class NativeBufferTests
    {
        [Fact]
        public void SetZeroCapacityFreesBuffer()
        {
            using (var buffer = new NativeBuffer(10))
            {
                buffer.Handle.Should().NotBe(IntPtr.Zero);
                buffer.Capacity = 0;
                buffer.Handle.Should().Be(IntPtr.Zero);
            }
        }

        [Fact]
        public void SetNegativeCapacityThrowsArgumentOutOfRange()
        {
            using (var buffer = new NativeBuffer())
            {
                Action action = () => { buffer.Capacity = -1; };
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void GetNegativeIndexThrowsArgumentOutOfRange()
        {
            using (var buffer = new NativeBuffer())
            {
                Action action = () => { byte c = buffer[-1]; };
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void GetOverIndexThrowsArgumentOutOfRange()
        {
            using (var buffer = new NativeBuffer())
            {
                Action action = () => { byte c = buffer[0]; };
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void SetNegativeIndexThrowsArgumentOutOfRange()
        {
            using (var buffer = new NativeBuffer())
            {
                Action action = () => { buffer[-1] = 0; };
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void SetOverIndexThrowsArgumentOutOfRange()
        {
            using (var buffer = new NativeBuffer())
            {
                Action action = () => { buffer[0] = 0; };
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void CanGetSetBytes()
        {
            using (var buffer = new NativeBuffer(1))
            {
                buffer[0] = 0xA;
                buffer[0].Should().Be(0xA);
            }
        }
    }
}
