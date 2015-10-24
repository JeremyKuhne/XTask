// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using Collections;

    /// <summary>
    /// Allows limited reuse of heap buffers to improve memory pressure. This cache does not ensure
    /// that multiple copies of handles are not released back into the cache.
    /// </summary>
    public sealed class HeapHandleCache : Cache<HeapHandle>
    {
        private ulong minSize;
        private ulong maxSize;

        private static readonly HeapHandleCache instance = new HeapHandleCache();

        public HeapHandleCache(ulong minSize = 64, ulong maxSize = 1024 * 2, int maxBuilders = 0)
            : base (cacheSpace: maxBuilders)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
        }

        public static HeapHandleCache Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Get a HeapHandle
        /// </summary>
        public HeapHandle Acquire(ulong minSize)
        {
            HeapHandle handle = this.Acquire();
            if (minSize < this.minSize) minSize = this.minSize;
            if (handle.ByteLength < minSize)
            {
                handle.Resize(minSize);
            }

            return handle;
        }

        protected override bool ShouldAttemptCache(HeapHandle item)
        {
            return item.ByteLength <= this.maxSize;
        }
    }
}
