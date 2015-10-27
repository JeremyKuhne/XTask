﻿// ----------------------
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
                buffer.DangerousGetHandle().Should().NotBe(IntPtr.Zero);
                buffer.EnsureByteCapacity(0);
                buffer.DangerousGetHandle().Should().NotBe(IntPtr.Zero);
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
                buffer.ByteCapacity.Should().Be(0);
                GetCurrentDirectorySafe((uint)buffer.ByteCapacity, buffer);
            }
        }

        [Fact]
        public void DisposedBufferIsEmpty()
        {
            var buffer = new NativeBuffer(5);
            buffer.ByteCapacity.Should().Be(5);
            buffer.Dispose();
            buffer.ByteCapacity.Should().Be(0);
            buffer.DangerousGetHandle().Should().Be(IntPtr.Zero);
        }

        [Fact]
        public void FreedBufferIsEmpty()
        {
            using (var buffer = new NativeBuffer(5))
            {
                buffer.ByteCapacity.Should().Be(5);
                buffer.Free();
                buffer.ByteCapacity.Should().Be(0);
                buffer.DangerousGetHandle().Should().Be(IntPtr.Zero);
            }
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364934.aspx
        [DllImport(Libraries.Kernel32, EntryPoint = "GetCurrentDirectoryW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern uint GetCurrentDirectorySafe(
            uint nBufferLength,
            SafeHandle lpBuffer);
            }
}
