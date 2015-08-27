// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Utility
{
    using System;
    using System.Linq;
    using FluentAssertions;
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
    }
}
