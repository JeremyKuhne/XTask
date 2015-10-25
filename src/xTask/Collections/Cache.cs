// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Collections
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public class Cache<T> : IDisposable where T : IDisposable, new() 
    {
        private int cacheSpace;
        private ConcurrentStack<T> itemsCache = new ConcurrentStack<T>();

        public Cache(int cacheSpace)
        {
            if (cacheSpace < 1) cacheSpace = Environment.ProcessorCount * 4;
            this.cacheSpace = cacheSpace;
        }

        public T Acquire()
        {
            T item;
            if (this.itemsCache.TryPop(out item))
            {
                Interlocked.Increment(ref this.cacheSpace);
            }
            else
            {
                item = new T();
            }

            return item;
        }

        public void Release(T item)
        {
            if (ShouldAttemptCache(item))
            {
                if (Interlocked.Decrement(ref this.cacheSpace) < 0)
                {
                    // No more space
                    Interlocked.Increment(ref this.cacheSpace);
                }
                else
                {
                    this.itemsCache.Push(this.PrepareCachedItem(item));
                    return;
                }
            }

            item.Dispose();
        }

        protected virtual T PrepareCachedItem(T item)
        {
            return item;
        }

        protected virtual bool ShouldAttemptCache(T item)
        {
            return true;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                T item;
                while (this.itemsCache.TryPop(out item)) item.Dispose();
                this.itemsCache = null;
            }
        }
    }
}
