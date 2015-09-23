// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Allows limited reuse of StringBuilders to improve memory pressure
    /// </summary>
    public class StringBuilderCache : IDisposable
    {
        private ThreadLocal<StringBuilder> stringBuilder;
        private int minSize;
        private int maxSize;

        public StringBuilderCache(int minSize = 16, int maxSize = 1024)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.stringBuilder = new ThreadLocal<StringBuilder>();
        }

        /// <summary>
        /// Get a StringBuilder
        /// </summary>
        public StringBuilder Acquire()
        {
            StringBuilder sb = this.stringBuilder.Value;
            if (sb == null)
            {
                sb = new StringBuilder(this.minSize);
            }
            else
            {
                sb.Clear();
                this.stringBuilder.Value = null;
            }
            return sb;
        }

        /// <summary>
        /// Give a StringBuilder back for potential reuse
        /// </summary>
        public void Release(StringBuilder sb)
        {
            if (sb.MaxCapacity <= this.maxSize)
            {
                this.stringBuilder.Value = sb;
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
                this.stringBuilder.Dispose();
            }
        }
    }
}
