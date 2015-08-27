// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper for access to the native heap. Dispose to free the memory. Try to use with using statements.
    /// </summary>
    public class NativeBuffer : IDisposable
    {
        // Heap Functions
        // --------------
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366711.aspx

        private static uint HEAP_NO_SERIALIZE = 0x00000001;
        private static uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
        private static uint HEAP_ZERO_MEMORY = 0x00000008;

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366597.aspx
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern HeapHandle HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366704.aspx
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern HeapHandle HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr lpMem, UIntPtr dwBytes);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366701.aspx
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366700.aspx
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapDestroy(IntPtr hHeap);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcessHeap();

        private static IntPtr ProcessHeap = GetProcessHeap();
        private HeapHandle handle;

        public IntPtr Handle
        {
            get
            {
                return this.handle?.DangerousGetHandle() ?? IntPtr.Zero;
            }
        }

        public int Size { get; private set; }

        public NativeBuffer(int initialSize = 0)
        {
            if (initialSize != 0)
            {
                this.Resize(initialSize);
            }
        }

        public static implicit operator IntPtr(NativeBuffer buffer)
        {
            return buffer.Handle;
        }

        public IntPtr EnsureCapacity(int size)
        {
            if (size < this.Size)
                return this.Handle;
            else
                return this.Resize(size);
        }

        public IntPtr Resize(int size)
        {

            HeapHandle newHandle = this.Handle == IntPtr.Zero
                ? HeapAlloc(ProcessHeap, 0, (UIntPtr)size)
                : HeapReAlloc(ProcessHeap, 0, this.Handle, (UIntPtr)size);

            if (newHandle.IsInvalid)
            {
                throw new InvalidOperationException("Could not allocate requested memory.");
            }

            this.handle = newHandle;
            this.Size = size;
            return this.Handle;
        }

        public void Dispose()
        {
            this.handle?.Dispose();
        }

        private class HeapHandle : SafeHandle
        {
            HeapHandle() : base (invalidHandleValue: IntPtr.Zero, ownsHandle: true)
            {
            }

            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            protected override bool ReleaseHandle()
            {
                return HeapFree(ProcessHeap, 0, this.handle);
            }
        }
    }
}
