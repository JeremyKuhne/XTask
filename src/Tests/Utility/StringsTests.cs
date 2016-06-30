// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Utility
{
    using FluentAssertions;
    using System;
    using XTask.Utility;
    using Xunit;

    public class StringsTests
    {
        [Fact]
        public void UnderlineNullString()
        {
            Strings.Underline(null).Should().Be(String.Empty);
        }

        [Fact]
        public void UnderlineEmptyString()
        {
            Strings.Underline(String.Empty).Should().Be(String.Empty);
        }

        [Fact]
        public void UnderlineSimpleString()
        {
            Strings.Underline("foo").Should().Be("foo\r\n---");
        }

        [Fact]
        public void UnderlineSpecifiedCharString()
        {
            Strings.Underline("foo", underlineCharacter: '=').Should().Be("foo\r\n===");
        }

        [Fact]
        public void UnderlineWordsString()
        {
            Strings.Underline("foo_bar").Should().Be("foo bar\r\n--- ---");
        }

        [Fact]
        public void UnderlineNoWordsString()
        {
            Strings.Underline("foo_bar", underlineCharacter: '-', breakCharacter: null).Should().Be("foo_bar\r\n-------");
        }

        [Fact]
        public void UnderlineWordsSpecifiedString()
        {
            Strings.Underline("foo_*bar", underlineCharacter: '-', breakCharacter: '*').Should().Be("foo_ bar\r\n---- ---");
        }

        [Theory,
            InlineData(null, null, true),
            InlineData(null, "", true),
            InlineData("", null, true),
            InlineData(null, "foo", false),
            InlineData("foo", null, false)]
        public void EqualsOrNoneNullArguments(string left, string right, bool expected)
        {
            Strings.EqualsOrNone(left, right).Should().Be(expected);
        }

        [Theory,
            InlineData(null),
            InlineData(""),
            InlineData("    ")]
        public void ValueOrNoneNullArguments(string value)
        {
            // XTask.XTaskStrings.Culture = CultureInfo.CurrentUICulture;
            Strings.ValueOrNone(value).Should().Be(XTask.XTaskStrings.NoValue);
        }

        [Theory,
            InlineData(null, ""),
            InlineData("", ""),
            InlineData("    ", ""),
            InlineData("Foo    Bar  FooBar", "Foo Bar FooBar")]
        public void CompressWhitespace(string value, string expected)
        {
            Strings.CompressWhiteSpace(value).Should().Be(expected);
        }

        [Theory,
            InlineData(null, " ", ""),
            InlineData("a\rb\nc\r\n\n\r\td", "", "abc\td"),
            InlineData("", " ", "")
            ]
        public void ReplaceLineEnds(string value, string replacement, string expected)
        {
            Strings.ReplaceLineEnds(value, replacement).Should().Be(expected);
        }

        [Theory,
            InlineData(null, ""),
            InlineData("", ""),
            InlineData("foo\tbar", "foo   bar"),
            InlineData("\tbar", "   bar"),
            InlineData("foo\t\t", "foo      ")]
        public void TabsToSpaces(string value, string expected)
        {
            Strings.TabsToSpaces(value).Should().Be(expected);
        }

        [Theory,
            InlineData(null, ""),
            InlineData(new string[] { "abc", "def" }, ""),
            InlineData(new string[] { "bar" }, "bar"),
            InlineData(new string[] { "bar", "bar" }, "bar"),
            InlineData(new string[] { "bar", "bar", "bar" }, "bar"),
            InlineData(new string[] { null, "boo", "bool" }, ""),
            InlineData(new string[] { "boo", "bool", null }, ""),
            InlineData(new string[] { "", "boo", "bool" }, ""),
            InlineData(new string[] { "foo", "boo", "bool" }, ""),
            InlineData(new string[] { "boo", "bool" }, "boo"),
            InlineData(new string[] { "boo", "bool", "bot" }, "bo"),
            InlineData(new string[] { "abcde", "abc", "abcd" }, "abc")]
        public void FindLeftmostString(string[] values, string expected)
        {
            Strings.FindLeftmostCommonString(values).Should().Be(expected);
        }

        [Theory,
            InlineData(null, ""),
            InlineData(new string[] { "abc", "def" }, ""),
            InlineData(new string[] { "bar" }, "bar"),
            InlineData(new string[] { "bar", "bar" }, "bar"),
            InlineData(new string[] { "bar", "bar", "bar" }, "bar"),
            InlineData(new string[] { null, "boo", "bool" }, ""),
            InlineData(new string[] { "oob", "loob", null }, ""),
            InlineData(new string[] { "", "oob", "loob" }, ""),
            InlineData(new string[] { "oof", "oob", "loob" }, ""),
            InlineData(new string[] { "oob", "loob" }, "oob"),
            InlineData(new string[] { "oob", "loob", "tob" }, "ob"),
            InlineData(new string[] { "edcba", "cba", "dcba" }, "cba")]
        public void FindRightmostString(string[] values, string expected)
        {
            Strings.FindRightmostCommonString(values).Should().Be(expected);
        }

        [Theory
            InlineData("", new string[] { })
            InlineData("\"", new string[] { })
            InlineData("  ", new string[] { })
            InlineData("f", new string[] { "f" })
            InlineData("f\"b", new string[] { "f\"b" })
            InlineData("f b", new string[] { "f", "b" })
            InlineData("f  b", new string[] { "f", "b" })
            InlineData("foo bar", new string[] { "foo", "bar" })
            InlineData("\"foo bar", new string[] { "foo bar" })
            InlineData("\"foo bar\"", new string[] { "foo bar" })
            InlineData("foo \"foo bar\" bar", new string[] { "foo", "foo bar", "bar" })
            ]
        public void SplitCommandLineTests(string value, string[] expected)
        {
            Strings.SplitCommandLine(value).ShouldAllBeEquivalentTo(expected);
        }

        [Fact]
        public void SplitThrowsForNullPointer()
        {
            Action action = () => Strings.Split(IntPtr.Zero, 0, ' ');
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void SplitThrowsForNegativeLength()
        {
            Action action = () => Strings.Split(new IntPtr(1), -1, ' ');
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SplitHandlesNoSplitCharacters()
        {
            Strings.Split(new IntPtr(1), 1, null).Should().BeEmpty("null should have nothing to split");
            Strings.Split(new IntPtr(1), 1, new char[0]).Should().BeEmpty("empty should have nothing to split");
        }

        [Theory
            InlineData("foo bar", new char[] { ' ' })
            InlineData("foobar", new char[] { ' ' })
            InlineData("foo bar ", new char[] { ' ' })
            InlineData("foobar ", new char[] { ' ' })
            InlineData(" ", new char[] { ' ' })
            InlineData("", new char[] { ' ' })
            ]
        unsafe public void SplitTestCases(string input, char[] splitChars)
        {
            fixed (void* start = input)
            {
                Strings.Split(new IntPtr(start), input.Length, splitChars).ShouldAllBeEquivalentTo(input.Split(splitChars));
            }
        }

        [Theory
            InlineData(@"", 0, @"", 0, StringComparison.Ordinal, 0)
            InlineData(@"A", 0, @"", 0, StringComparison.OrdinalIgnoreCase, 0)
            InlineData(@"", 0, @"A", 0, StringComparison.InvariantCulture, 0)
            InlineData(@"A", 1, @"A", 0, StringComparison.InvariantCultureIgnoreCase, 0)
            InlineData(@"A", 0, @"A", 1, StringComparison.CurrentCulture, 0)
            InlineData(@"A", 0, @"A", 0, StringComparison.Ordinal, 1)
            InlineData(@"a", 0, @"A", 0, StringComparison.Ordinal, 0)
            InlineData(@"a", 0, @"A", 0, StringComparison.OrdinalIgnoreCase, 1)
            InlineData(@"AA", 0, @"A", 0, StringComparison.Ordinal, 1)
            InlineData(@"A", 0, @"AA", 0, StringComparison.Ordinal, 1)
            InlineData(@"A", 0, @"aA", 0, StringComparison.Ordinal, 0)
            InlineData(@"A", 0, @"aA", 1, StringComparison.Ordinal, 1)
            InlineData(@"AA", 0, @"AA", 0, StringComparison.Ordinal, 1)
            InlineData(@"AA", 1, @"AA", 0, StringComparison.Ordinal, 1)
            InlineData(@"AA", 1, @"AA", 1, StringComparison.Ordinal, 2)
            InlineData(@"AA", 0, @"AA", 1, StringComparison.Ordinal, 1)
            InlineData(@"aA", 0, @"AA", 0, StringComparison.Ordinal, 0)
            InlineData(@"aA", 1, @"AA", 1, StringComparison.Ordinal, 1)
            InlineData(@"aA", 2, @"AA", 1, StringComparison.Ordinal, 0)
            InlineData(@"aA", 2, @"AA", 2, StringComparison.Ordinal, 0)
            InlineData(@"AA", 0, @"aA", 0, StringComparison.Ordinal, 0)
            InlineData(@"AA", 1, @"aA", 0, StringComparison.Ordinal, 0)
            InlineData(@"AA", 0, @"aA", 1, StringComparison.Ordinal, 1)
            InlineData(@"AA", 1, @"aA", 1, StringComparison.Ordinal, 1)
            InlineData(@"AA", 0, @"aA", 0, StringComparison.OrdinalIgnoreCase, 1)
            InlineData(@"AA", 1, @"aA", 1, StringComparison.OrdinalIgnoreCase, 2)
            ]
        public void FindRightmostCommonCountTests(string first, int firstIndex, string second, int secondIndex, StringComparison comparisonType, int expected)
        {
            Strings.FindRightmostCommonCount(
                first: first,
                firstIndex: firstIndex,
                second: second,
                secondIndex: secondIndex,
                comparisonType: comparisonType).Should().Be(expected);
        }

        [Theory
            InlineData(null, "")
            InlineData("", null)
            InlineData(null, null)
            ]
        public void FindRightmostCommonCountArgumentNull(string first, string second)
        {
            Action action = () => Strings.FindRightmostCommonCount(first, 0, second, 0);
        }


        [Theory,
            InlineData(null, null, true, 0)
            InlineData(null, null, false, 0)
            InlineData(null, "", true, 0)
            InlineData(null, "", false, 0)
            InlineData("", "", true, 0)
            InlineData("", "", false, 0)
            InlineData("a", "a", true, 1)
            InlineData("a", "a", false, 1)
            InlineData("A", "a", true, 1)
            InlineData("A", "a", false, 0)
            InlineData("foo", "foobar", true, 3)
            InlineData("foo", "foobar", false, 3)
            InlineData("foo", "foOBar", true, 3)
            InlineData("foo", "foOBar", false, 2)
            ]
        public void FindLeftmostCommonCount(string first, string second, bool ignoreCase, int expected)
        {
            Strings.FindLeftmostCommonCount(first, second, ignoreCase).Should().Be(expected);
            Strings.FindLeftmostCommonCount(second, first, ignoreCase).Should().Be(expected, "order shouldn't matter");
        }
    }
}
