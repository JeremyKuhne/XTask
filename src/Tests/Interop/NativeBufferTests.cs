// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using System.Runtime.InteropServices;
    using XTask.Interop;
    using Xunit;
    using static XTask.Interop.NativeMethods;

    public class NativeBufferTests
    {
        [Fact]
        public void EnsureZeroCapacityDoesNotFreeBuffer()
        {
            using (var buffer = new NativeBuffer(10))
            {
                ((IntPtr)buffer).Should().NotBe(IntPtr.Zero);
                buffer.EnsureCapacity(0);
                ((IntPtr)buffer).Should().NotBe(IntPtr.Zero);
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

        [Fact]
        public void NullSafePointerInTest()
        {
            using (var buffer = new NativeBuffer(0))
            {
                ((SafeHandle)buffer).IsInvalid.Should().BeTrue();
                buffer.Capacity.Should().Be(0);
                GetCurrentDirectorySafe((uint)buffer.Capacity, buffer);
            }
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364934.aspx
        [DllImport(Libraries.Kernel32, EntryPoint = "GetCurrentDirectoryW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern uint GetCurrentDirectorySafe(
            uint nBufferLength,
            SafeHandle lpBuffer);
            }
}
