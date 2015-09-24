// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.Collections.Concurrent;
    using System.Text;

    /// <summary>
    /// Allows limited reuse of StringBuilders to improve memory pressure
    /// </summary>
    public class StringBuilderCache : IDisposable
    {
        internal static StringBuilderCache Instance = new StringBuilderCache();

        private uint minSize;
        private uint maxSize;
        private uint maxBuilders;

        private ConcurrentBag<StringBuilder> builders;

        public StringBuilderCache(uint minSize = 16, uint maxSize = 1024, uint maxBuilders = 1024)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.maxBuilders = maxBuilders;
            this.builders = new ConcurrentBag<StringBuilder>();
        }

        /// <summary>
        /// Get a StringBuilder
        /// </summary>
        public StringBuilder Acquire()
        {
            StringBuilder sb;
            if (builders.TryTake(out sb))
            {
                sb.Clear();
            }
            else
            {
                sb = new StringBuilder((int)this.minSize);
            }

            return sb;
        }

        /// <summary>
        /// Give a StringBuilder back for potential reuse
        /// </summary>
        public void Release(StringBuilder sb)
        {
            if (sb.MaxCapacity <= this.maxSize && this.builders.Count < maxBuilders)
            {
                this.builders.Add(sb);
            }
        }

        /// <summary>
        /// Give a StringBuilder back for potential reuse
        /// </summary>
        public string ToStringAndRelease(StringBuilder sb)
        {
            string value = sb.ToString();
            this.Release(sb);
            return value;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StringBuilder sb;
                while (this.builders.TryTake(out sb));
                this.builders = null;
            }
        }
    }
}
