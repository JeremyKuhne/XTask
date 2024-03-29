﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Collections;

namespace XTask.Tests.Collections;

public class EnumerableExtensionsTests
{
    private bool IsGreaterThanZero(int value)
    {
        return value > 0;
    }

    [Fact]
    public void WhereNotThrowsOnNullTest()
    {
        IEnumerable<int> source = null;
        Action action = () => source.WhereNot(IsGreaterThanZero);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("source");

        source = Enumerable.Empty<int>();
        action = () => source.WhereNot(null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("predicate");
    }

    [Fact]
    public void WhereNotTest()
    {
        int[] source = { -1, 3, 0, 2 };
        source.WhereNot(IsGreaterThanZero).Should().ContainInOrder(-1, 0);
    }

    [Fact]
    public void ConcatManyThrowsOnNullTest()
    {
        IEnumerable<int> first = null;
        IEnumerable<IEnumerable<int>> second = null;
        Action action = () => first.Concat(second);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("first");

        first = Enumerable.Empty<int>();
        action = () => first.Concat(second);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("second");
    }

    [Fact]
    public void ConcatManyTest()
    {
        int[] first = { 1, 2 };
        int[] second = { 3, 4 };
        int[] third = { 5, 6, 7 };
        int[][] fourth = { second, third };

        first.Concat(fourth).Should().ContainInOrder(1, 2, 3, 4, 5, 6, 7);
    }
}
