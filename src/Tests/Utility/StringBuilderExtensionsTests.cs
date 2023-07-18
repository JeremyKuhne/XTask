// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Logging;
using XTask.Utility;

namespace XTask.Tests.Utility;

public class StringBuilderExtensionsTests
{
    [Theory,
        InlineData("Foo", Justification.Left, 6, "Foo   "),
        InlineData("Foo", Justification.Right, 6, "   Foo"),
        InlineData("Foo", Justification.Centered, 6, " Foo  "),
        InlineData("Foo", Justification.Centered, 7, "  Foo  "),
        InlineData("Foo", Justification.Left, 3, "Foo"),
        InlineData("Foo", Justification.Right, 3, "Foo"),
        InlineData("Foo", Justification.Centered, 3, "Foo"),
        InlineData("FooBar", Justification.Left, 4, "FooB"),
        InlineData("FooBar", Justification.Right, 4, "FooB"),
        InlineData("FooBar", Justification.Centered, 4, "FooB")]
    public void WriteColumn(string value, Justification justification, int width, string expected)
    {
        StringBuilder sb = new();
        sb.WriteColumn(value, justification, width);
        sb.ToString().Should().Be(expected, "'{0}' with column width of {1}, {2} justified", value, width, justification);
    }

    [Theory,
        InlineData("Foo", Justification.Left, 6, "Foo"),
        InlineData("Foo", Justification.Right, 6, "   Foo"),
        InlineData("Foo", Justification.Centered, 6, " Foo"),
        InlineData("Foo", Justification.Centered, 7, "  Foo"),
        InlineData("Foo", Justification.Left, 3, "Foo"),
        InlineData("Foo", Justification.Right, 3, "Foo"),
        InlineData("Foo", Justification.Centered, 3, "Foo"),
        InlineData("FooBar", Justification.Left, 4, "FooB"),
        InlineData("FooBar", Justification.Right, 4, "FooB"),
        InlineData("FooBar", Justification.Centered, 4, "FooB")]
    public void WriteColumnNoRightPadding(string value, Justification justification, int width, string expected)
    {
        StringBuilder sb = new();
        sb.WriteColumn(value, justification, width, noRightPadding: true);
        sb.ToString().Should().Be(expected, "specified '{0}' with column width of {1}, {2} justified", value, width, justification);
    }

    [Theory,
        InlineData("", new char[] { }, new string[] { "" }),
        InlineData("foo:bar", new char[] { ':' }, new string[] { "foo", "bar" }),
        InlineData(":foo:bar", new char[] { ':' }, new string[] { "", "foo", "bar" }),
        InlineData("foo::bar", new char[] { ':' }, new string[] { "foo", "", "bar" }),
        InlineData("foo:bar:", new char[] { ':' }, new string[] { "foo", "bar", "" }),
        InlineData("foo\0bar", new char[] { '\0' }, new string[] { "foo", "bar" })
        ]
    public void SplitTests(string value, char[] splitCharacters, string[] expected)
    {
        StringBuilder sb = new(value);
        sb.Split(splitCharacters).Should().BeEquivalentTo(expected);
    }
}
