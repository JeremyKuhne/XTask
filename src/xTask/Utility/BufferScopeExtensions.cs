// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable


namespace Windows.Support;

internal static class BufferScopeExtensions
{
    /// <summary>
    ///  Returns the buffer as a string with the given length. Will use the <paramref name="existing"/>
    ///  string if it matches.
    /// </summary>
    public static string ToString(in this BufferScope<char> buffer, uint length, string existing)
    {
        if (length > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        Span<char> result = buffer.Slice(0, (int)length);
        return result.SequenceEqual(existing.AsSpan()) ? existing : result.ToString();
    }

    /// <summary>
    ///  Returns the buffer as a string, terminating at the first null.
    /// </summary>
    public static string ToStringAtNull(in this BufferScope<char> buffer) => buffer.SliceAtNull().ToString();

    /// <summary>
    ///  Slices the buffer at the first <see langword="null"/> or returns the entire length if <see langword="null"/>
    ///  is not found.
    /// </summary>
    public static Span<char> SliceAtNull(in this BufferScope<char> buffer)
    {
        Span<char> span = buffer.AsSpan();
        int nullIndex = span.IndexOf('\0');
        return nullIndex == -1 ? span : span.Slice(0, nullIndex + 1);
    }
}