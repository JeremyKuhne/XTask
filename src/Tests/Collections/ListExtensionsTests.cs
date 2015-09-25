// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Collections
{
    using System.Collections.Generic;
    using FluentAssertions;
    using XTask.Collections;
    using Xunit;

    public class ListExtensionsTests
    {
        [Fact]
        public void Shuffle_HandlesNull()
        {
            IList<int> test = null;
            test.Shuffle();
        }

        [Fact]
        public void Shuffle_OrderChanges()
        {
            // In theory we could get back the exact same order- really unlikely, particularly with larger collections
            int[] source = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<int> items = new List<int>(source);
            items.Shuffle();
            items.Should().NotEqual(source);
        }
    }
}
