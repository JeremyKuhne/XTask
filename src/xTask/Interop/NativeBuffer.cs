// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Wrapper for access to the native heap. Dispose to free the memory. Try to use with using statements.
    /// </summary>
    public class NativeBuffer : Stream
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
            if (this.Length < value  || this.stream == null)
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
            this.stream = new UnmanagedMemoryStream(
                pointer: (byte*)this.Handle.ToPointer(),
                length: size,
                capacity: size,
                access: FileAccess.ReadWrite);

            return this.Handle;
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "stream")]
        protected override void Dispose(bool disposing)
        {
            disposed = true;
            this.handle?.Dispose();

            if (disposing)
            {
                this.stream?.Dispose();
                this.stream = null;
            }
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
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            if (this.stream == null)
            {
                // Mimic UnmanagedMemoryStream with a 0 length buffer
                if (offset != 0) throw new ArgumentException();
            }

            this.EnsureLength(count + offset);
            this.stream.Write(buffer, offset, count);
        }

        private class HeapHandle : SafeHandleZeroIsInvalid
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
