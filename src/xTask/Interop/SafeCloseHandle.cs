// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;

    public class SafeCloseHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeCloseHandle() : base(ownsHandle: true) { }

        public SafeCloseHandle(IntPtr handle) : base(ownsHandle: true)
        {
            this.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CloseHandle(this.handle);
            return true;
        }
    }
}
