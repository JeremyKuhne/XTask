// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Collections;

namespace XTask.Interop
{
    /// <summary>
    /// Allows caching of StringBuffer objects to ease GC pressure when creating many StringBuffers.
    /// </summary>
    public sealed class StringBufferCache : Cache<StringBuffer>
    {
        private static readonly StringBufferCache instance = new StringBufferCache(0);

        public StringBufferCache(int maxBuffers) : base(maxBuffers)
        {
        }

        public static StringBufferCache Instance
        {
            get { return instance; }
        }

        protected override StringBuffer PrepareCachedItem(StringBuffer item)
        {
            // Free the underlying buffer (which is implicitly cached)
            item.Free();
            return item;
        }
    }
}
