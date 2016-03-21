﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Runtime.InteropServices;

    public class SafeNetApiBufferHandle : SafeBuffer
    {
        internal SafeNetApiBufferHandle() : base(ownsHandle: true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.NetworkManagement.NetApiBufferFree(this);
            handle = IntPtr.Zero;
            return true;
        }
    }
}
