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

        private int _minCapcity;
        private int _maxCapacity;

        /// <summary>
        /// Create a StringBuilder cache.
        /// </summary>
        /// <param name="minCapacity">The minimum capacity for created StringBuilders.</param>
        /// <param name="maxCapacity">The maximum capacity for cached StringBuilders.</param>
        /// <param name="maxBuilders">The maximum number of builders to cache. If less than one scales to the number of processors.</param>
        public StringBuilderCache(int minCapacity = 16, int maxCapacity = 1024, int maxBuilders = 0)
            : base(maxBuilders)
        {
            if (minCapacity < 0) minCapacity = 0;
            if (maxCapacity < 0) maxCapacity = 0;
            _minCapcity = minCapacity;
            _maxCapacity = maxCapacity;
        }

        public override StringBuilder Acquire()
        {
            var builder = base.Acquire();
            builder.EnsureCapacity(_minCapcity);
            return builder;
        }

        /// <summary>
        /// Acquire a StringBuilder with at least the specified capacity.
        /// </summary>
        public StringBuilder Acquire(int minCapacity)
        {
            var builder = base.Acquire();
            builder.EnsureCapacity(minCapacity);
            return builder;
        }

        public override void Release(StringBuilder item)
        {
            item.Clear();
            if (item.Capacity <= _maxCapacity)
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
