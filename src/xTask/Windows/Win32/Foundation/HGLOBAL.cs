// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Windows.Win32.Foundation;

internal unsafe partial struct HGLOBAL : IDisposable
{
    public void Dispose()
    {
        if (Value is not null)
        {
            Interop.GlobalFree(this);
        }
    }
}
