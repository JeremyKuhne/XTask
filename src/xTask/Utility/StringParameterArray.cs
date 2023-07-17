// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace xTask.Utility;

/// <summary>
///  Helper to allow pinning an array of strings to pass to a native method that takes an
///  array of null-terminated UTF-16 strings.
/// </summary>
internal unsafe readonly ref struct StringParameterArray
{
    private readonly GCHandle[]? _pins;
    private readonly GCHandle _pin;
    private readonly nint[]? _param;

    public StringParameterArray(string[]? values)
    {
        int length = values?.Length ?? 0;
        if (length > 0)
        {
            _param = new nint[length];
            _pin = GCHandle.Alloc(_param, GCHandleType.Pinned);
            _pins = new GCHandle[length];
            for (int i = 0; i < length; i++)
            {
                _pins[i] = GCHandle.Alloc(values![i], GCHandleType.Pinned);
                _param[i] = _pins[i].AddrOfPinnedObject();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator char**(in StringParameterArray array)
        => array._param is null ? null : (char**)Unsafe.AsPointer(ref Unsafe.AsRef(array._param[0]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator sbyte**(in StringParameterArray array)
        => array._param is null ? null : (sbyte**)Unsafe.AsPointer(ref Unsafe.AsRef(array._param[0]));

    public void Dispose()
    {
        if (_pins is null)
        {
            return;
        }

        _pin.Free();

        for (int i = 0; i < _pins.Length; i++)
        {
            _pins[i].Free();
        }
    }
}