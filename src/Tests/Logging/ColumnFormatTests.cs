// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using XTask.Logging;
using Xunit;

namespace XTask.Tests.Logging
{
    public class ColumnFormatTests
    {
        [Fact]
        public void FromCountConstructor()
        {
            ColumnFormat[] baseLine = { new ColumnFormat(1, ContentVisibility.ShowAll), new ColumnFormat(1), new ColumnFormat(1) };
            ColumnFormat[] columns = ColumnFormat.FromCount(3);
            columns.Should().BeEquivalentTo(baseLine, "as specified in helper constuctor");
        }

        [Fact]
        public void FromWidthConstructor()
        {
            ColumnFormat[] baseLine = { new ColumnFormat(4, ContentVisibility.ShowAll), new ColumnFormat(6) };
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, -6);
            columns.Should().BeEquivalentTo(baseLine, "as specified in helper constuctor");
        }

        [Fact]
        public void SimpleScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(100, columns);
            evenScale.Should().ContainInOrder(40, 60);
        }

        [Fact]
        public void NoScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(10, columns);
            evenScale.Should().ContainInOrder(4, 6);
        }

        [Fact]
        public void RemainderScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(11, columns);
            evenScale.Should().ContainInOrder(4, 7);
        }

        [Fact]
        public void ReducedScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(5, columns);
            evenScale.Should().ContainInOrder(2, 3);
        }

        [Fact]
        public void NegativeScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(-10, columns);
            evenScale.Should().ContainInOrder(4, 6);
        }

        [Fact]
        public void SmallestWidthScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(2, columns);
            evenScale.Should().ContainInOrder(1, 1);
        }

        [Fact]
        public void IdentityScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(1, 1, 1);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(3, columns);
            evenScale.Should().ContainInOrder(1, 1, 1);
        }

        [Theory,
            InlineData(99, 1, 1),
            InlineData(1, 2, 2),
            InlineData(1, 2, 3),
            InlineData(3, 2, 1),
            InlineData(1, 99, 1)]
        public void DistributedMinimumScale(int first, int second, int third)
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(first, second, third);
            int[] evenScale = ColumnFormat.ScaleColumnWidths(3, columns);
            evenScale.Should().ContainInOrder(1, 1, 1);
        }

        [Fact]
        public void TooSmallWidthScale()
        {
            ColumnFormat[] columns = ColumnFormat.FromWidths(4, 6);
            Action action = () => ColumnFormat.ScaleColumnWidths(1, columns);
            action.Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("fullWidth");
        }
    }
}
