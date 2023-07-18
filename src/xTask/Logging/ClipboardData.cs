// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;

namespace XTask.Logging;

public readonly struct ClipboardData
{
    private readonly object? _data;
    public readonly ClipboardFormat Format;

    public ClipboardData(ReadOnlyMemory<char> data, ClipboardFormat format)
    {
        _data = data;
        Format = format;
    }

    public ClipboardData(ReadOnlyMemory<byte> data, ClipboardFormat format)
    {
        _data = data;
        Format = format;
    }

    public bool HasData => _data is not null;

    public ReadOnlySpan<char> CharData => _data is ReadOnlyMemory<char> memory ? memory.Span : default;
    public ReadOnlySpan<byte> ByteData => _data is ReadOnlyMemory<byte> memory ? memory.Span : default;

}
