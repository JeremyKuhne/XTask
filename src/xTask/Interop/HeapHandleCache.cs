// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Allows limited reuse of heap buffers to improve memory pressure. This cache does not ensure
    /// that multiple copies of handles are not released back into the cache.
    /// </summary>
    public class HeapHandleCache : IDisposable
    {
        internal static HeapHandleCache Instance = new HeapHandleCache();

        private int minSize;
        private uint maxSize;
        private uint maxBuilders;

        private ConcurrentBag<HeapHandle> buffers;

        public HeapHandleCache(int minSize = 64, uint maxSize = 1024 * 4, uint maxBuilders = 0)
        {
            this.minSize = minSize < 0 ? 0 : minSize;
            this.maxSize = maxSize;
            this.maxBuilders = maxBuilders > 1 ? maxBuilders : (uint)Environment.ProcessorCount * 4;
            this.buffers = new ConcurrentBag<HeapHandle>();
        }

        /// <summary>
        /// Get a HeapHandle
        /// </summary>
        public HeapHandle Acquire(uint minCapacity = 0)
        {
            HeapHandle buffer;
            if (buffers.TryTake(out buffer))
            {
                if ((uint)buffer.Size < minCapacity)
                {
                    buffer.Resize((UIntPtr)minCapacity);
                }
            }
            else
            {
                buffer = new HeapHandle(minCapacity);
            }

            return buffer;
        }

        /// <summary>
        /// Give a HeapHandle back for potential reuse
        /// </summary>
        public void Release(HeapHandle buffer)
        {
            if ((uint)buffer.Size <= this.maxSize && this.buffers.Count < maxBuilders)
            {
                this.buffers.Add(buffer);
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                HeapHandle buffer;
                while (this.buffers.TryTake(out buffer)) ;
                this.buffers = null;
            }
        }
    }
}
