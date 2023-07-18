// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Utility;

namespace XTask.Tests.Utility;

public class ArraysTests
{
    [Theory,
        InlineData(null, null, true),
        InlineData(null, new string[0], true),
        InlineData(new string[0], null, true),
        InlineData(new string[0], new string[0], true),
        InlineData(new string[0], new string[] { "foo" }, false),
        InlineData(null, new string[] { "foo" }, false),
        InlineData(new string[] { "foo" }, new string[0], false),
        InlineData(new string[] { "foo" }, null, false),
        InlineData(new string[] { "foo" }, new string[] { "foo" }, true),
        InlineData(new string[] { "foo" }, new string[] { "bar" }, false),
        InlineData(new string[] { "foo", "bar" }, new string[] { "foo", "bar" }, true),
        InlineData(new string[] { "foo", "bar" }, new string[] { "foo", "bar", "foobar" }, false),
        InlineData(new string[] { "foo", "bar" }, new string[] { "bar", "foo" }, false)]
    public void ReferenceArrayEquivalencyTest(string[] left, string[] right, bool expected)
    {
        Arrays.AreEquivalent(left, right).Should().Be(expected);
    }

    [Theory,
        InlineData(new int[] { 1 }, new int[0], false),
        InlineData(new int[] { 1 }, new int[] { 1 }, true),
        InlineData(new int[] { 1 }, new int[] { 2 }, false),
        InlineData(new int[] { 1, 2 }, new int[] { 1, 2 }, true),
        InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2 }, false),
        InlineData(new int[] { 1, 2 }, new int[] { 2, 1 }, false)]
    public void ValueArrayEquivalencyTest(int[] left, int[] right, bool expected)
    {
        Arrays.AreEquivalent(left, right).Should().Be(expected);
    }

    [Theory,
        InlineData(null, "<null>"),
        InlineData(new string[0], "<empty>"),
        InlineData(new string[] { "foo" }, "foo"),
        InlineData(new string[] { "foo", "bar" }, "foo bar")]
    public void ReferenceArrayCreateStringTest(string[] input, string expected)
    {
        Arrays.CreateString(input).Should().Be(expected);
    }
}
