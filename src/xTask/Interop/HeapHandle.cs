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
    /// <remarks>
    /// Uses new Heap* methods instead of Local* methods, which are depreciated.  While both calls utilize the same underlying
    /// heap allocation, Local* adds some overhead (*significant* overhead if LMEM_MOVEABLE is used). .NET forces LMEM_FIXED
    /// with LocalAlloc for Marshal.AllocHGlobal so it doesn't hit the super slow path.
    /// 
    /// Windows attempts to grab space from the low fragmentation heap if the requested memory is below a platform specific
    /// threshold and certain flags aren't in play (such as NO_SERIALIZE).
    /// </remarks>
    public class HeapHandle : SafeBuffer
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
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = false, ExactSpelling = true)]
            internal static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366704.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = false, ExactSpelling = true)]
            internal static extern IntPtr HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr lpMem, UIntPtr dwBytes);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366701.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366706.aspx
            //[DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            //internal static extern UIntPtr HeapSize(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366700.aspx
            //[DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            //internal static extern bool HeapDestroy(IntPtr hHeap);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366598.aspx
            //[DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            //internal static extern UIntPtr HeapCompact(IntPtr hHeap, uint dwFlags);

            // This is safe to cache as it will never change for a process once started
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366569.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern IntPtr GetProcessHeap();
        }

        /// <summary>
        /// The handle for the process heap.
        /// </summary>
        public static IntPtr ProcessHeap = NativeMethods.GetProcessHeap();

        /// <summary>
        /// Allocate a buffer of the given size and zero memory if requested.
        /// </summary>
        /// <param name="byteLength">Required size in bytes. Must be less than UInt32.MaxValue for 32 bit or UInt64.MaxValue for 64 bit.</param>
        /// <exception cref="OutOfMemoryException">Thrown if the requested memory size cannot be allocated.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if size is greater than the maximum memory size.</exception>
        public HeapHandle(ulong byteLength, bool zeroMemory = false) : base(ownsHandle: true)
        {
            this.Resize(byteLength, zeroMemory);
        }

        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        /// <summary>
        /// Resize the buffer to the given size and zero memory if requested.
        /// </summary>
        /// <param name="byteLength">Required size in bytes. Must be less than UInt32.MaxValue for 32 bit or UInt64.MaxValue for 64 bit.</param>
        /// <exception cref="OutOfMemoryException">Thrown if the requested memory size cannot be allocated.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if size is greater than the maximum memory size.</exception>
        public void Resize(ulong byteLength, bool zeroMemory = false)
        {
            if (this.IsClosed) throw new ObjectDisposedException("HeapHandle");

            uint flags = zeroMemory ? NativeMethods.HEAP_ZERO_MEMORY : 0;

            if (this.handle == IntPtr.Zero)
            {
                this.handle = NativeMethods.HeapAlloc(ProcessHeap, flags, (UIntPtr)byteLength);
            }
            else
            {
                // This may or may not be the same handle, Windows may realloc in place. If the
                // handle changes Windows will deal with the old handle, trying to free it will
                // cause an error.
                this.handle = NativeMethods.HeapReAlloc(ProcessHeap, flags, this.handle, (UIntPtr)byteLength);
            }

            if (this.handle == IntPtr.Zero)
            {
                // Only real plausible answer
                throw new OutOfMemoryException();
            }

            this.Initialize(byteLength);
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
