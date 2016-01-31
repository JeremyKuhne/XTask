// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using Collections;
    using System.Text;

    /// <summary>
    /// Allows limited reuse of StringBuilders to improve memory pressure
    /// </summary>
    public class StringBuilderCache : Cache<StringBuilder>
    {
        internal static StringBuilderCache Instance = new StringBuilderCache();

        private int _minSize;
        private int _maxSize;

        public StringBuilderCache(int minSize = 16, int maxSize = 1024, int maxBuilders = 0)
            : base(maxBuilders)
        {
            if (minSize < 0) minSize = 0;
            if (maxSize < 0) maxSize = 0;
            _minSize = minSize;
            _maxSize = maxSize;
        }

        public override StringBuilder Acquire()
        {
            var builder = base.Acquire();
            builder.EnsureCapacity(_minSize);
            return builder;
        }

        public override void Release(StringBuilder item)
        {
            item.Clear();
            if (item.Capacity <= _maxSize)
            {
                base.Release(item);
            }
        }

        /// <summary>
        /// Give a StringBuilder back for potential reuse and return it's contents as a string
        /// </summary>
        public string ToStringAndRelease(StringBuilder sb)
        {
            string value = sb.ToString();
            Release(sb);
            return value;
        }
    }
}
