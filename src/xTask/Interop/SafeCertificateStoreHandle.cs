﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;

    public class SafeCertificateStoreHandle : SafeHandleZeroIsInvalid
    {
        internal SafeCertificateStoreHandle() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Cryptography.CloseStore(this);
            handle = IntPtr.Zero;
            return true;
        }
    }
}
