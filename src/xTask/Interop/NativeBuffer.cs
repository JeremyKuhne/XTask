﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper for access to the native heap. Dispose to free the memory. Try to use with using statements.
    /// Does not allocate zero size buffers, and will free the existing native buffer if capacity is dropped to zero.
    /// 
    /// NativeBuffer utilizes a cache of heap buffers.
    /// </summary>
    /// <remarks>
    /// Suggested use through P/Invoke: define DllImport arguments that take a byte buffer as SafeHandle.
    /// 
    /// Using SafeHandle will ensure that the buffer will not get collected during a P/Invoke but introduces very slight
    /// overhead. (Notably AddRef and ReleaseRef will be called by the interop layer.)
    /// 
    /// This class is not threadsafe, changing the capacity or disposing on multiple threads risks duplicate heap
    /// handles or worse.
    /// </remarks>
    public class NativeBuffer : IDisposable
    {
        private HeapHandle handle;
        private ulong capacity;

        /// <summary>
        /// Create a buffer with at least the specified initial capacity in bytes.
        /// </summary>
        public NativeBuffer(ulong initialMinCapacity = 0)
        {
            this.EnsureByteCapacity(initialMinCapacity);
        }

        protected unsafe void* VoidPointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.InternalDangerousGetHandle().ToPointer();
            }
        }

        protected unsafe byte* BytePointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte*)this.VoidPointer;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr InternalDangerousGetHandle()
        {
            SafeHandle handle = this.handle;
            return handle == null ? IntPtr.Zero : handle.DangerousGetHandle();
        }

        public IntPtr DangerousGetHandle()
        {
            return InternalDangerousGetHandle();
        }

        public static implicit operator SafeHandle(NativeBuffer buffer)
        {
            // Marshalling code will throw on null for SafeHandle
            return buffer.handle ?? EmptySafeHandle.Instance;
        }

        /// <summary>
        /// The capacity of the buffer in bytes.
        /// </summary>
        public ulong ByteCapacity
        {
            get { return this.capacity; }
        }

        /// <summary>
        /// Ensure capacity in bytes is at least the given minimum.
        /// </summary>
        /// <exception cref="OutOfMemoryException">Thrown if unable to allocate memory when setting.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set <paramref name="nameof(minCapacity)"/> to a value that is larger than the maximum addressable memory.</exception>
        public void EnsureByteCapacity(ulong minCapacity)
        {
            if (this.capacity < minCapacity)
            {
                this.Resize(minCapacity);
                this.capacity = minCapacity;
            }
        }

        public unsafe byte this[ulong index]
        {
            get
            {
                if (index >= this.capacity) throw new ArgumentOutOfRangeException();
                return BytePointer[index];
            }
            set
            {
                if (index >= this.capacity) throw new ArgumentOutOfRangeException();
                BytePointer[index] = value;
            }
        }

        private unsafe void Resize(ulong byteLength)
        {
            if (byteLength == 0)
            {
                this.ReleaseHandle();
                return;
            }

            if (this.handle == null)
            {
                this.handle = HeapHandleCache.Instance.Acquire(byteLength);
            }
            else
            {
                this.handle.Resize(byteLength);
            }
        }

        private void ReleaseHandle()
        {
            if (this.handle != null)
            {
                HeapHandleCache.Instance.Release(this.handle);
                this.handle = null;
            }
        }

        /// <summary>
        /// Free the space used by the buffer. Sets capacity to 0.
        /// </summary>
        public virtual void Free()
        {
            this.ReleaseHandle();
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.Free();
        }
    }
}
