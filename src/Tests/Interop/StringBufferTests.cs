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

    public class StringBufferTests
    {
        const string testString = "The quick brown fox jumped over the lazy dog.";

        [Fact]
        public void CanIndexChar()
        {
            using (var buffer = new StringBuffer())
            {
                buffer.Length = 1;
                buffer[0] = 'Q';
                buffer[0].Should().Be('Q');
            }
        }

        [Fact]
        public unsafe void CreateFromString()
        {
            string testString = "Test";
            using (var buffer = new StringBuffer(testString))
            {
                buffer.Length.Should().Be(testString.Length);
                buffer.Capacity.Should().Be(testString.Length + 1);

                for (int i = 0; i < testString.Length; i++)
                {
                    buffer[i].Should().Be(testString[i]);
                }

                ((char*)buffer.Handle.ToPointer())[testString.Length].Should().Be('\0', "should be null terminated");

                buffer.ToString().Should().Be(testString);
            }
        }

        [Fact]
        public void NegativeLengthThrowsArgumentOutOfRange()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Length = -1;
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void NegativeCapacityThrowsArgumentOutOfRange()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Capacity = -1;
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void OverIntCapacityThrowsArgumentOutOfRange()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Capacity = (long)int.MaxValue + 1;
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void NegativeInitialCapacityThrowsArgumentOutOfRange()
        {
            Action action = () => new StringBuffer(initialLength: -1);
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Theory
            InlineData(@"Foo", @"Foo", true)
            InlineData(@"Foo", @"foo", false)
            InlineData(@"Foobar", @"Foo", true)
            InlineData(@"Foobar", @"foo", false)
            InlineData(@"Fo", @"Foo", false)
            InlineData(@"Fo", @"foo", false)
            InlineData(@"", @"", true)
            InlineData(@"", @"f", false)
            InlineData(@"f", @"", true)
            ]
        public void StartsWithOrdinal(string source, string value, bool expected)
        {
            using (var buffer = new StringBuffer(source))
            {
                buffer.StartsWithOrdinal(value).Should().Be(expected);
            }
        }

        [Fact]
        public void StartsWithNullThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.StartsWithOrdinal(null);
                action.ShouldThrow<ArgumentNullException>();
            }
        }

        [Fact]
        public void SubStringEqualsNegativeIndexThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.SubStringEquals("", startIndex: -1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void SubStringEqualsNegativeCountThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.SubStringEquals("", startIndex: 0, count: -2);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void SubStringEqualsOverSizeCountThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.SubStringEquals("", startIndex: 0, count: 1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void SubStringEqualsOverSizeCountWithIndexThrows()
        {
            using (var buffer = new StringBuffer("A"))
            {
                Action action = () => buffer.SubStringEquals("", startIndex: 1, count: 1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Theory
            InlineData(@"", @"", 0, 0, true)
            InlineData(@"", @"", 0, -1, true)
            InlineData(@"A", @"", 0, -1, false)
            InlineData(@"", @"A", 0, -1, false)
            InlineData(@"Foo", @"Foo", 0, -1, true)
            InlineData(@"Foo", @"foo", 0, -1, false)
            InlineData(@"Foo", @"Foo", 1, -1, false)
            InlineData(@"Foo", @"Food", 0, -1, false)
            InlineData(@"Food", @"Foo", 0, -1, false)
            InlineData(@"Food", @"Foo", 0, 3, true)
            InlineData(@"Food", @"ood", 1, 3, true)
            InlineData(@"Food", @"ooD", 1, 3, false)
            InlineData(@"Food", @"ood", 1, 2, false)
            InlineData(@"Food", @"Food", 0, 3, false)
        ]
        public void SubStringEquals(string source, string value, int startIndex, int count, bool expected)
        {
            using (var buffer = new StringBuffer(source))
            {
                buffer.SubStringEquals(value, startIndex: startIndex, count: count).Should().Be(expected);
            }
        }

        [Fact]
        public void AppendNullThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Append(null);
                action.ShouldThrow<ArgumentNullException>();
            }
        }

        [Fact]
        public void AppendNegativeIndexThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Append("", startIndex: -1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void AppendOverIndexThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Append("", startIndex: 1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void AppendNegativeCountThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Append("", startIndex: 0, count: -2);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void AppendOverCountThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Append("", startIndex: 0, count: 1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void AppendOverCountWithIndexThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.Append("A", startIndex: 1, count: 1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void ToStringNegativeIndexThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.ToString(startIndex: -1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void ToStringIndexOverLengthThrows()
        {
            using (var buffer = new StringBuffer())
            {
                Action action = () => buffer.ToString(startIndex: 1);
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        [Theory
            InlineData(@"", 0, -1, @"")
            InlineData(@"A", 0, -1, @"A")
            InlineData(@"AB", 0, -1, @"AB")
            InlineData(@"AB", 0, 1, @"A")
            InlineData(@"AB", 1, 1, @"B")
            InlineData(@"AB", 1, -1, @"B")
            InlineData(@"", 0, 0, @"")
            InlineData(@"A", 0, 0, @"")
        ]
        public void ToStringTest(string source, int startIndex, int count, string expected)
        {
            using (var buffer = new StringBuffer(source))
            {
                buffer.ToString(startIndex: startIndex, count: count).Should().Be(expected);
            }
        }

        [Fact]
        public unsafe void SetLengthToFirstNullNoNull()
        {
            using (var buffer = new StringBuffer("A"))
            {
                // Wipe out the last null
                ((char*)buffer.Handle.ToPointer())[buffer.Length] = 'B';
                buffer.SetLengthToFirstNull();
                buffer.Length.Should().Be(1);
            }
        }

        [Fact]
        public unsafe void SetLengthToFirstNullEmptyBuffer()
        {
            using (var buffer = new StringBuffer())
            {
                buffer.SetLengthToFirstNull();
                buffer.Length.Should().Be(0);
            }
        }

        [Theory
            InlineData(@"", 0, 0)
            InlineData(@"Foo", 3, 3)
            InlineData("\0", 1, 0)
            InlineData("Foo\0Bar", 7, 3)
            ]
        public unsafe void SetLengthToFirstNullTests(string content, int startLength, int endLength)
        {
            using (var buffer = new StringBuffer(content))
            {
                // With existing content
                buffer.Length.Should().Be(startLength);
                buffer.SetLengthToFirstNull();
                buffer.Length.Should().Be(endLength);

                // Clear the buffer & manually copy in
                buffer.Length = 0;
                fixed (char* contentPointer = content)
                {
                    Buffer.MemoryCopy(contentPointer, buffer.Handle.ToPointer(), buffer.Capacity * 2, content.Length * sizeof(char));
                }

                buffer.Length.Should().Be(0);
                buffer.SetLengthToFirstNull();
                buffer.Length.Should().Be(endLength);
            }
        }

        [Theory
            InlineData("foo bar", ' ')
            InlineData("foo\0bar", '\0')
            InlineData("foo\0bar", ' ')
            InlineData("foobar", ' ')
            InlineData("foo bar ", ' ')
            InlineData("foobar ", ' ')
            InlineData("foobar ", 'b')
            InlineData(" ", ' ')
            InlineData("", ' ')
            InlineData(null, ' ')
            ]
        public void Split(string content, char splitChar)
        {
            // We want equivalence with built-in string behavior
            using (var buffer = new StringBuffer(content))
            {
                buffer.Split(splitChar).ShouldAllBeEquivalentTo(content?.Split(splitChar) ?? new string[] { "" });
            }
        }

        [Theory
            InlineData("foo bar", new char[] { ' ' })
            InlineData("foo bar", new char[] { })
            InlineData("foo\0bar", new char[] { '\0' })
            InlineData("foo\0bar", new char[] { ' ' })
            InlineData("foobar", new char[] { ' ' })
            InlineData("foo bar ", new char[] { ' ' })
            InlineData("foobar ", new char[] { ' ' })
            InlineData("foobar ", new char[] { ' ', 'b' })
            InlineData(" ", new char[] { ' ' })
            InlineData("", new char[] { ' ' })
            InlineData(null, new char[] { ' ' })
            ]
        public void SplitParams(string content, char[] splitChars)
        {
            // We want equivalence with built-in string behavior
            using (var buffer = new StringBuffer(content))
            {
                buffer.Split(splitChars).ShouldAllBeEquivalentTo(content?.Split(splitChars) ?? new string[] { "" });
            }
        }

        [Theory
            InlineData(null, ' ', false)
            InlineData("", ' ', false)
            InlineData("foo", 'F', false)
            InlineData("foo", '\0', false)
            InlineData("foo", 'f', true)
            InlineData("foo", 'o', true)
            InlineData("foo\0", '\0', true)
            ]
        public void ContainsTests(string content, char value, bool expected)
        {
            using (var buffer = new StringBuffer(content))
            {
                buffer.Contains(value).Should().Be(expected);
            }
        }

        [Theory
            InlineData(null, null, false)
            InlineData(null, new char[0], false)
            InlineData(null, new char[] { ' ' }, false)
            InlineData("", new char[] { ' ' }, false)
            InlineData("foo", new char[] { 'F' }, false)
            InlineData("foo", new char[] { '\0' }, false)
            InlineData("foo", new char[] { 'f' }, true)
            InlineData("foo", new char[] { 'o' }, true)
            InlineData("foo\0", new char[] { '\0' }, true)
            ]
        public void ContainsParamsTests(string content, char[] values, bool expected)
        {
            using (var buffer = new StringBuffer(content))
            {
                buffer.Contains(values).Should().Be(expected);
            }
        }
    }
}
