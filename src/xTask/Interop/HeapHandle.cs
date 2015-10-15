// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Handle for heap memory
    /// </summary>
    public class HeapHandle : SafeHandleZeroIsInvalid
    {
        [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
        protected static class NativeMethods
        {
            // Heap Functions
            // --------------
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366711.aspx

            //internal const uint HEAP_NO_SERIALIZE = 0x00000001;
            //internal const uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
            internal const uint HEAP_ZERO_MEMORY = 0x00000008;
            //internal const uint HEAP_REALLOC_IN_PLACE_ONLY = 0x00000010;

            // HeapAlloc/Realloc take a SIZE_T for their count of bytes. This is ultimately an
            // unsigned __int3264 which is platform specific (uint on 32bit and ulong on 64bit).
            // UIntPtr can encapsulate this as it wraps void* and has unsigned constructors.
            // (IntPtr also wraps void*, but uses signed constructors.)
            // 
            // SIZE_T:
            // https://msdn.microsoft.com/en-us/library/cc441980.aspx

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366597.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366704.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern IntPtr HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr lpMem, UIntPtr dwBytes);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366701.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366700.aspx
            //[DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            //internal static extern bool HeapDestroy(IntPtr hHeap);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366569.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern IntPtr GetProcessHeap();
        }

        protected static IntPtr ProcessHeap = NativeMethods.GetProcessHeap();

        /// <summary>
        /// Allocate a buffer of the given size and zero memory if requested.
        /// </summary>
        /// <exception cref="OutOfMemoryException">Thrown if the requested memory size cannot be allocated.</exception>
        public HeapHandle(uint size, bool zeroMemory = false) : this((UIntPtr)size, zeroMemory)
        {
        }

        /// <summary>
        /// Allocate a buffer of the given size and zero memory if requested.
        /// </summary>
        /// <exception cref="OutOfMemoryException">Thrown if the requested memory size cannot be allocated.</exception>
        public HeapHandle(UIntPtr size, bool zeroMemory = false) : base(ownsHandle: true)
        {
            this.Resize(size, zeroMemory);
        }

        /// <summary>
        /// Resize the buffer to the given size and zero memory if requested.
        /// </summary>
        /// <exception cref="OutOfMemoryException">Thrown if the requested memory size cannot be allocated.</exception>
        public void Resize(UIntPtr size, bool zeroMemory = false)
        {
            if (this.IsClosed) throw new ObjectDisposedException("HeapHandle");

            uint flags = zeroMemory ? NativeMethods.HEAP_ZERO_MEMORY : 0;

            if (this.handle == IntPtr.Zero)
            {
                this.handle = NativeMethods.HeapAlloc(ProcessHeap, flags, size);
            }
            else
            {
                // This may or may not be the same handle, Windows may realloc in place
                this.handle = NativeMethods.HeapReAlloc(ProcessHeap, flags, this.handle, size);
            }

            if (this.handle == IntPtr.Zero)
            {
                // Only real plausible answer
                throw new OutOfMemoryException();
            }
        }

        protected override bool ReleaseHandle()
        {
            bool success = NativeMethods.HeapFree(ProcessHeap, 0, this.handle);
            Debug.Assert(success);
            this.handle = IntPtr.Zero;
            return success;
        }
    }
}
