// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using XTask.Collections;
    using Xunit;

    public class CacheTests
    {
        public class TestItem : IDisposable
        {
            public void Dispose() { }
        }

        public class TestCache : Cache<TestItem>
        {
            public int CachedCount;

            public TestCache(int cacheSpace) : base(cacheSpace)
            {
            }

            protected override TestItem PrepareCachedItem(TestItem item)
            {
                CachedCount++;
                return item;
            }
        }

        [Fact]
        public void CachedItemCountTest()
        {
            TestCache cache = new TestCache(5);
            TestItem item = new TestItem();
            for (int i = 0; i < 7; i++)
            {
                cache.Release(item);
            }

            cache.CachedCount.Should().Be(5);
        }

        [Fact]
        public void GetCachedItem()
        {
            TestCache cache = new TestCache(5);
            TestItem item = new TestItem();
            cache.Release(item);
            cache.Acquire().Should().BeSameAs(item);
            cache.Acquire().Should().NotBeSameAs(item);
        }

        [Fact]
        public void CachedItemParallelCountTest()
        {
            TestCache cache = new TestCache(5);
            TestItem item = new TestItem();
            Parallel.For(0, 10000, (i) => cache.Release(item));
            cache.CachedCount.Should().Be(5);
        }
    }
}
