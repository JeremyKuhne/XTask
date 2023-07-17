// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;

namespace Windows.Support;

internal static class SpanExtensions
{
    public static Span<char> SliceAtNull(this Span<char> span)
    {
        int length = span.IndexOf('\0');
        return length < 0 ? span : span.Slice(0, length);
    }

    /// <inheritdoc cref="Split(in ReadOnlySpan{char}, char, bool)">
    public static IEnumerable<string> Split(in this Span<char> span, char splitCharacter, bool includeEmptyStrings = false)
        => ((ReadOnlySpan<char>)span).Split(splitCharacter, includeEmptyStrings);

    /// <summary>
    ///  Split the buffer contents into strings via the given split characters.
    /// </summary>
    public static IEnumerable<string> Split(in this ReadOnlySpan<char> span, char splitCharacter, bool includeEmptyStrings = false)
    {
        List<string> strings = new();

        ReadOnlySpan<char> current = span;

        int index;
        while ((index = current.IndexOf(splitCharacter)) != -1)
        {
            if (index > 0)
            {
                strings.Add(current.Slice(0, index).ToString());
            }
            else if (includeEmptyStrings)
            {
                strings.Add(string.Empty);
            }

            current = current.Slice(index + 1);
        }

        if (current.Length > 0)
        {
            strings.Add(current.ToString());
        }

        return strings;
    }

}
