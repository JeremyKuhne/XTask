// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace XTask.Collections
{
    public static class ListExtensions
    {
        private static readonly Random s_Random = new();

        /// <summary>
        ///  Shuffles the elements in the given list. (Fisher-Yates)
        /// </summary>
        public static void Shuffle<TSource>(this IList<TSource> source)
        {
            if (source is null || source.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                int j = s_Random.Next(i, source.Count);
                (source[j], source[i]) = (source[i], source[j]);
            }
        }
    }
}
