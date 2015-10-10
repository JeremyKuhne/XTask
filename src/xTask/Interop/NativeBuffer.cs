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
    /// Wrapper for access to the native heap. Dispose to free the memory. Try to use with using statements.
    /// </summary>
    /// <remarks>
    /// Suggested use through P/Invoke: define DllImport arguments that take a byte buffer as IntPtr.
    /// NativeBuffer has an implicit conversion to IntPtr.
    /// </remarks>
    public class NativeBuffer : IDisposable
    {
        [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
        private static class NativeMethods
        {
            // Heap Functions
            // --------------
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366711.aspx

            // private static uint HEAP_NO_SERIALIZE = 0x00000001;
            // private static uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
            // private static uint HEAP_ZERO_MEMORY = 0x00000008;
            // private static uint HEAP_REALLOC_IN_PLACE_ONLY = 0x00000010;

            // HeapAlloc/Realloc take a SIZE_T for their count of bytes. This is ultimately an
            // unsigned __int3264 which is platform specific (uint on 32bit and ulong on 64bit).
            // UIntPtr can encapsulate this as it wraps void* and has unsigned constructors.
            // (IntPtr also wraps void*, but uses signed constructors.)
            // 
            // SIZE_T:
            // https://msdn.microsoft.com/en-us/library/cc441980.aspx

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366597.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern HeapHandle HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366704.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern HeapHandle HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr lpMem, UIntPtr dwBytes);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366701.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366700.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern bool HeapDestroy(IntPtr hHeap);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366569.aspx
            [DllImport(Interop.NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern IntPtr GetProcessHeap();
        }

        private static IntPtr ProcessHeap = NativeMethods.GetProcessHeap();
        protected HeapHandle handle;
        private long capacity;

        public NativeBuffer(uint initialCapacity = 0)
        {
            if (initialCapacity != 0)
            {
                this.Resize(initialCapacity);
            }
        }

        public IntPtr Handle
        {
            get
            {
                return this.handle?.DangerousGetHandle() ?? IntPtr.Zero;
            }
        }

        public static implicit operator IntPtr(NativeBuffer buffer)
        {
            return buffer.Handle;
        }

        /// <summary>
        /// The capacity of the buffer in bytes.
        /// </summary>
        public virtual long Capacity
        {
            get { return this.capacity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                this.Resize(value);
                this.capacity = value;
            }
        }

        /// <summary>
        /// Ensure the buffer has at least the specified capacity.
        /// </summary>
        public void EnsureCapacity(long capacity)
        {
            if (this.Capacity < capacity)
            {
                this.Capacity = capacity;
            }
        }

        public unsafe byte this[long index]
        {
            get
            {
                if (index < 0 || index >= this.Capacity) throw new ArgumentOutOfRangeException();
                return ((byte*)handle.DangerousGetHandle())[index];
            }
            set
            {
                if (index < 0 || index >= this.Capacity) throw new ArgumentOutOfRangeException();
                ((byte*)handle.DangerousGetHandle())[index] = value;
            }
        }

        unsafe private IntPtr Resize(long size)
        {
            HeapHandle newHandle = (this.Handle == IntPtr.Zero)
                ? NativeMethods.HeapAlloc(ProcessHeap, 0, (UIntPtr)size)
                : NativeMethods.HeapReAlloc(ProcessHeap, 0, this.Handle, (UIntPtr)size);

            if (newHandle.IsInvalid)
            {
                throw new InvalidOperationException("Could not allocate requested memory.");
            }

            if (this.handle != null)
            {
                // Since we've reallocated, we don't need to free the existing handle
                this.handle.SetHandleAsInvalid();
            }

            this.handle = newHandle;
            return this.Handle;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.handle?.Dispose();
        }

        protected class HeapHandle : SafeHandleZeroIsInvalid
        {
            HeapHandle() : base (ownsHandle: true)
            {
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
}
