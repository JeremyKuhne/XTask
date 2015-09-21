// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper for access to the native heap. Dispose to free the memory. Try to use with using statements.
    /// </summary>
    public class NativeBuffer : Stream
    {
        // Heap Functions
        // --------------
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366711.aspx

        private static uint HEAP_NO_SERIALIZE = 0x00000001;
        private static uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
        private static uint HEAP_ZERO_MEMORY = 0x00000008;
        private static uint HEAP_REALLOC_IN_PLACE_ONLY = 0x00000010;

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366597.aspx
        [DllImport(NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
        private static extern HeapHandle HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366704.aspx
        [DllImport(NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
        private static extern HeapHandle HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr lpMem, UIntPtr dwBytes);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366701.aspx
        [DllImport(NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
        private static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366700.aspx
        [DllImport(NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
        private static extern bool HeapDestroy(IntPtr hHeap);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366569.aspx
        [DllImport(NativeMethods.Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcessHeap();

        private static IntPtr ProcessHeap = GetProcessHeap();
        private HeapHandle handle;
        private UnmanagedMemoryStream stream;
        private bool disposed;

        public NativeBuffer(uint initialLength = 0)
        {
            if (initialLength != 0)
            {
                this.Resize(initialLength);
            }
        }

        public IntPtr Handle
        {
            get
            {
                return this.handle?.DangerousGetHandle() ?? IntPtr.Zero;
            }
        }

        public override bool CanRead
        {
            get { return this.stream?.CanRead ?? !disposed; }
        }

        public override bool CanSeek
        {
            get { return this.stream?.CanSeek ?? !disposed; }
        }

        public override bool CanWrite
        {
            get { return this.stream?.CanWrite ?? !disposed; }
        }

        public override long Length
        {
            get { return this.stream?.Length ?? 0; }
        }

        public override long Position
        {
            get
            {
                return this.stream?.Position ?? 0;
            }
            set
            {
                if (value < 0 || value > Length) throw new ArgumentOutOfRangeException(nameof(value));
                if (this.Position != value)
                    this.stream.Position = value;
            }
        }

        public static implicit operator IntPtr(NativeBuffer buffer)
        {
            return buffer.Handle;
        }

        public void EnsureLength(long value)
        {
            if (this.Length < value)
            {
                this.Resize(value);
            }
        }

        public override void SetLength(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            this.Resize(value);
        }

        unsafe private IntPtr Resize(long size)
        {
            HeapHandle newHandle = (this.Handle == IntPtr.Zero)
                ? HeapAlloc(ProcessHeap, 0, (UIntPtr)size)
                : HeapReAlloc(ProcessHeap, 0, this.Handle, (UIntPtr)size);

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
            this.stream = new UnmanagedMemoryStream((byte*)this.Handle.ToPointer(), size);
            return this.Handle;
        }

        protected override void Dispose(bool disposing)
        {
            disposed = true;
            this.handle?.Dispose();
        }

        public override void Flush()
        {
            this.stream?.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.stream == null)
            {
                // Only 0 makes any sense, otherwise throw IOException like UnmanagedMemoryStream would
                if (offset != 0) throw new IOException();
                else return 0;
            }

            return this.stream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.stream == null)
            {
                // Mimic UnmanagedMemoryStream with a 0 length buffer
                if (buffer == null) throw new ArgumentNullException(nameof(buffer));
                if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
                if (offset != 0 || count != 0) throw new ArgumentException();
                return 0;
            }

            return this.stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.stream == null)
            {
                // Mimic UnmanagedMemoryStream with a 0 length buffer
                if (buffer == null) throw new ArgumentNullException(nameof(buffer));
                if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
                if (offset != 0 || count != 0) throw new ArgumentException();
                return;
            }

            this.stream.Write(buffer, offset, count);
        }

        private class HeapHandle : SafeHandleZeroIsInvalid
        {
            HeapHandle() : base (ownsHandle: true)
            {
            }

            protected override bool ReleaseHandle()
            {
                bool success = HeapFree(ProcessHeap, 0, this.handle);
                Debug.Assert(success);
                this.handle = IntPtr.Zero;
                return success;
            }
        }
    }
}
