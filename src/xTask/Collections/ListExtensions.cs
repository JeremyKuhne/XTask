// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Collections
{
    using System;
    using System.Collections.Generic;

    public static class ListExtensions
    {
        private static Random s_Random = new Random();

        /// <summary>
        /// Shuffles the elements in the given list. (Fisher-Yates)
        /// </summary>
        public static void Shuffle<TSource>(this IList<TSource> source)
        {
            if (source == null || source.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                int j = s_Random.Next(i, source.Count);
                TSource temp = source[i];
                source[i] = source[j];
                source[j] = temp;
            }
        }
    }
}
