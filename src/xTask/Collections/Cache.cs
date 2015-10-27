// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Collections
{
    using System;
    using System.Threading;

    public class Cache<T> : IDisposable where T : class, IDisposable, new()
    {
        protected readonly T[] itemsCache;

        public Cache(int cacheSpace)
        {
            if (cacheSpace < 1) cacheSpace = Environment.ProcessorCount * 4;
            this.itemsCache = new T[cacheSpace];
        }

        public T Acquire()
        {
            T item;

            for (int i = 0; i < this.itemsCache.Length; i++)
            {
                item = Interlocked.Exchange(ref this.itemsCache[i], null);
                if (item != null) return item;
            }

            return new T();
        }

        public virtual void Release(T item)
        {
            for (int i = 0; i < this.itemsCache.Length; i++)
            {
                item = Interlocked.Exchange(ref this.itemsCache[i], item);
                if (item == null) return;
            }

            item.Dispose();
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
                    this.itemsCache[i]?.Dispose();
                    this.itemsCache[i] = null;
                }
            }
        }
    }
}
