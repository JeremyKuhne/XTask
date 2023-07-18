// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Collections;

public static class EnumerableExtensions
{
    /// <summary>
    ///  Filters a sequence of values based on the inverse of a predicate
    /// </summary>
    /// <example>
    ///  Where not allows for terser syntax when you already have a matching predicate.
    ///  <code>
    ///   foreach (string path in pathsToCheck.WhereNot(String.IsNullOrWhiteSpace))
    ///  </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
    public static IEnumerable<TSource> WhereNot<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        if (source is null) { throw new ArgumentNullException(nameof(source)); }
        if (predicate is null) { throw new ArgumentNullException(nameof(predicate)); }

        return source.Where(item => !predicate(item));
    }

    /// <summary>
    ///  Concats a sequence of sequence of values with another sequence.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="first"/> or <paramref name="second"/> is null.</exception>
    public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<IEnumerable<TSource>> second)
    {
        if (first is null) { throw new ArgumentNullException(nameof(first)); }
        if (second is null) { throw new ArgumentNullException(nameof(second)); }

        return ConcatManyIterator(first, second);
    }

    private static IEnumerable<TSource> ConcatManyIterator<TSource>(IEnumerable<TSource> first, IEnumerable<IEnumerable<TSource>> second)
    {
        foreach (TSource element in first) yield return element;

        foreach (IEnumerable<TSource> enumerable in second)
            foreach (TSource element in enumerable) yield return element;
    }

    /// <summary>
    ///  Perform a series of specified actions in turn against the given source. First action is performed against
    ///  the first source item, the second against the second, etc.
    /// </summary>
    public static void ForEachDoOne<TSource>(this IEnumerable<TSource> source, params Action<TSource>[] actions)
    {
        int index = 0;
        foreach (var item in source)
        {
            actions[index](item);
            index++;
        }
    }
}