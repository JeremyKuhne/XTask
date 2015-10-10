// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Allows limited reuse of NativeStringBuffers to improve memory pressure
    /// </summary>
    public class StringBufferCache : IDisposable
    {
        internal static StringBufferCache Instance = new StringBufferCache();

        private int minSize;
        private uint maxSize;
        private uint maxBuilders;

        private ConcurrentBag<StringBuffer> buffers;

        public StringBufferCache(int minSize = 64, uint maxSize = 1024 * 4, uint maxBuilders = 0)
        {
            this.minSize = minSize < 0 ? 0 : minSize;
            this.maxSize = maxSize;
            this.maxBuilders = maxBuilders > 1 ? maxBuilders : (uint)Environment.ProcessorCount * 4;
            this.buffers = new ConcurrentBag<StringBuffer>();
        }

        /// <summary>
        /// Get a NativeStringBuffer
        /// </summary>
        public StringBuffer Acquire(int minCapacity = 0)
        {
            StringBuffer buffer;
            if (buffers.TryTake(out buffer))
            {
                buffer.Length = 0;
                buffer.EnsureCapacity(minCapacity);
            }
            else
            {
                buffer = new StringBuffer(Math.Max(this.minSize, minCapacity));
            }

            return buffer;
        }

        /// <summary>
        /// Give a NativeStringBuffer back for potential reuse
        /// </summary>
        public void Release(StringBuffer buffer)
        {
            if (buffer.Capacity <= this.maxSize && this.buffers.Count < maxBuilders)
            {
                this.buffers.Add(buffer);
            }
        }

        /// <summary>
        /// Give a NativeStringBuffer back for potential reuse and return it's contents as a string
        /// </summary>
        public string ToStringAndRelease(StringBuffer buffer, int startIndex = 0, int count = -1)
        {
            string value = buffer.ToString(startIndex: startIndex, count: count);
            this.Release(buffer);
            return value;
        }

        /// <summary>
        /// Give a NativeStringBuffer back for potential reuse and return it's contents split on nulls
        /// </summary>
        public IEnumerable<string> ToStringsAndRelease(StringBuffer buffer)
        {
            IEnumerable<string> values = buffer.Split('\0');
            this.Release(buffer);
            return values;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StringBuffer buffer;
                while (this.buffers.TryTake(out buffer)) ;
                this.buffers = null;
            }
        }
    }
}
