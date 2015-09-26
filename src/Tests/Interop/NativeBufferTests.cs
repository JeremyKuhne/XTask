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

    public class NativeBufferTests
    {
        const string testString = "The quick brown fox jumped over the lazy dog.";

        [Fact]
        public void EmptyBufferHasZeroLength()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Length.Should().Be(0);
            }
        }

        [Fact]
        public void EmptyBufferPositionIsZero()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Position.Should().Be(0);
            }
        }

        [Fact]
        public void EmptyBufferCanSetPositionToZero()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Position = 0;
                buffer.Position.Should().Be(0);
            }
        }

        [Fact]
        public void PositionCannotBeSetOutsideLength()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action under = () => buffer.Position = -1;
                under.ShouldThrow<ArgumentOutOfRangeException>();
                Action over = () => buffer.Position = 1;
                over.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void EmptyBufferCanRead()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.CanRead.Should().BeTrue();
            }
        }

        [Fact]
        public void DisposedEmptyBufferCannotRead()
        {
            NativeBuffer buffer;
            using (buffer = new NativeBuffer(0))
            {
            }
            buffer.CanRead.Should().BeFalse();
        }

        [Fact]
        public void EmptyBufferCanSeek()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.CanSeek.Should().BeTrue();
            }
        }

        [Fact]
        public void DisposedEmptyBufferCannotSeek()
        {
            NativeBuffer buffer;
            using (buffer = new NativeBuffer(0))
            {
            }
            buffer.CanSeek.Should().BeFalse();
        }

        [Fact]
        public void EmptyBufferCanWrite()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.CanWrite.Should().BeTrue();
            }
        }

        [Fact]
        public void DisposedEmptyBufferCannotWrite()
        {
            NativeBuffer buffer;
            using (buffer = new NativeBuffer(0))
            {
            }
            buffer.CanWrite.Should().BeFalse();
        }

        [Fact]
        public void EmptyBufferCanFlush()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Flush();
            }
        }

        [Fact]
        public void EmptyBufferCanReadNothing()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Read(new byte[0], 0, 0).Should().Be(0);
            }
        }

        [Fact]
        public void EmptyBufferCanWriteNothing()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Write(new byte[0], 0, 0);
            }
        }

        [Fact]
        public void EmptyBufferCanSeekNowhere()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                buffer.Seek(0, SeekOrigin.Begin).Should().Be(0);
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnSeek()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Seek(1, SeekOrigin.Begin);
                action.ShouldThrow<IOException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnNullBufferWrite()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Write(null, 0, 0);
                action.ShouldThrow<ArgumentNullException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnNullBufferRead()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Read(null, 0, 0);
                action.ShouldThrow<ArgumentNullException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnNegativeOffsetWrite()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Write(new byte[0], -1, 0);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnNegativeOffsetRead()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Read(new byte[0], -1, 0);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnNegativeCountWrite()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Write(new byte[0], 0, -1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnNegativeCountRead()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Read(new byte[0], 0, -1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnPositiveOffsetWrite()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Write(new byte[0], 1, 0);
                action.ShouldThrow<ArgumentException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnPositiveOffsetRead()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Read(new byte[0], 1, 0);
                action.ShouldThrow<ArgumentException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnPositiveCountWrite()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Write(new byte[0], 0, 1);
                action.ShouldThrow<ArgumentException>();
            }
        }

        [Fact]
        public void EmptyBufferThrowsOnPositiveCountRead()
        {
            using (NativeBuffer buffer = new NativeBuffer(0))
            {
                Action action = () => buffer.Read(new byte[0], 0, 1);
                action.ShouldThrow<ArgumentException>();
            }
        }

        [Fact]
        public void StreamWriterOnEmptyBuffer()
        {
            using (NativeBuffer buffer = new NativeBuffer())
            {
                using (StreamWriter writer = new StreamWriter(buffer))
                using (StreamReader reader = new StreamReader(buffer))
                {
                    writer.AutoFlush = true;
                    writer.WriteLine(testString);
                    reader.BaseStream.Position = 0;
                    reader.ReadLine().Should().Be(testString);
                }
            }
        }

        [Fact]
        public void StreamWriterSetLengthToZero()
        {
            using (NativeBuffer buffer = new NativeBuffer())
            {
                using (StreamWriter writer = new StreamWriter(buffer))
                using (StreamReader reader = new StreamReader(buffer))
                {
                    writer.AutoFlush = true;
                    writer.WriteLine(testString);
                    reader.BaseStream.Position = 0;
                    reader.ReadLine().Should().Be(testString);
                    writer.BaseStream.SetLength(0);
                    reader.ReadLine().Should().BeNull();
                }
            }
        }
    }
}
