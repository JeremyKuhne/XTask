// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Collections
{
    using System;
    using System.Threading;

    /// <summary>
    /// Light weight multithreaded fixed size cache class.
    /// </summary>
    public class Cache<T> : IDisposable where T : class, new()
    {
        // Protected for testing
        protected readonly T[] itemsCache;

        /// <summary>
        /// Create a cache with space for the specified number of items.
        /// </summary>
        public Cache(int cacheSpace)
        {
            if (cacheSpace < 1) cacheSpace = Environment.ProcessorCount * 4;
            this.itemsCache = new T[cacheSpace];
        }

        /// <summary>
        /// Get an item from the cache or create one if none are available.
        /// </summary>
        public virtual T Acquire()
        {
            T item;

            for (int i = 0; i < this.itemsCache.Length; i++)
            {
                item = Interlocked.Exchange(ref this.itemsCache[i], null);
                if (item != null) return item;
            }

            return new T();
        }

        /// <summary>
        /// Release an item back to the cache, disposing if no room is available.
        /// </summary>
        public virtual void Release(T item)
        {
            for (int i = 0; i < this.itemsCache.Length; i++)
            {
                item = Interlocked.Exchange(ref this.itemsCache[i], item);
                if (item == null) return;
            }

            (item as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < this.itemsCache.Length; i++)
                {
                    (this.itemsCache[i] as IDisposable)?.Dispose();
                    this.itemsCache[i] = null;
                }
            }
        }
    }
}
