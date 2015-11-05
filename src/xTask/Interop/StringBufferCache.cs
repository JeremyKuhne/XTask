﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using XTask.Collections;

    /// <summary>
    /// Allows caching of StringBuffer objects to ease GC pressure when creating many StringBuffers.
    /// </summary>
    public sealed class StringBufferCache : Cache<StringBuffer>
    {
        private static readonly StringBufferCache instance = new StringBufferCache(0);

        public StringBufferCache(int maxBuffers) : base(maxBuffers)
        {
        }

        public StringBuffer Acquire(ulong minCapacity)
        {
            StringBuffer item = this.Acquire();
            item.EnsureCharCapacity(minCapacity: minCapacity);
            return item;
        }

        public static StringBufferCache Instance
        {
            get { return instance; }
        }

        public override void Release(StringBuffer item)
        {
            // Free the underlying buffer (which is implicitly cached)
            item.Free();
            base.Release(item);
        }

        public string ToStringAndRelease(StringBuffer item)
        {
            string returnValue = item.ToString();
            this.Release(item);
            return returnValue;
        }

        /// <summary>
        /// Invoke the given action on a cached buffer.
        /// </summary>
        public static void CachedBufferInvoke(Action<StringBuffer> action)
        {
            var buffer = Instance.Acquire();
            try
            {
                action(buffer);
            }
            finally
            {
                Instance.Release(buffer);
            }
        }

        public static T CachedBufferInvoke<T>(Func<StringBuffer, T> func)
        {
            return CachedBufferInvoke(0, func);
        }

        public static T CachedBufferInvoke<T>(uint minCapacity, Func<StringBuffer, T> func)
        {
            var buffer = Instance.Acquire(minCapacity);
            try
            {
                return func(buffer);
            }
            finally
            {
                Instance.Release(buffer);
            }
        }
    }
}
