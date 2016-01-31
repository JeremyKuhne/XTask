// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;

    public class SafeLibraryHandle : SafeHandleZeroIsInvalid
    {
        internal SafeLibraryHandle() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            bool success = NativeMethods.FreeLibrary(this);
            handle = IntPtr.Zero;
            return success;
        }
    }
}
