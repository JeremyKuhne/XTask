// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper for access to the native heap. Dispose to free the memory. Try to use with using statements.
    /// Does not allocate zero size buffers, and will free the existing native buffer if capacity is dropped to zero.
    /// 
    /// NativeBuffer utilizes a cache of heap buffers.
    /// </summary>
    /// <remarks>
    /// Suggested use through P/Invoke: define DllImport arguments that take a byte buffer as SafeHandle or IntPtr.
    /// NativeBuffer has an implicit conversion for both.
    /// 
    /// Using SafeHandle will ensure that the buffer will not get collected during a P/Invoke but introduces some overhead.
    /// (Notably AddRef and ReleaseRef will be called by the interop layer.)
    /// 
    /// This class is not threadsafe, changing the capacity or disposing on multiple threads risks duplicate heap
    /// handles or worse.
    /// </remarks>
    public class NativeBuffer : IDisposable
    {
        private static SafeHandle EmptyHandle = new EmptySafeHandle();
        private HeapHandle handle;
        private long capacity;

        public NativeBuffer(uint initialCapacity = 0)
        {
            this.Capacity = initialCapacity;
        }

        protected unsafe void* VoidPointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.handle == null ? null : this.handle.DangerousGetHandle().ToPointer();
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

        public static implicit operator IntPtr(NativeBuffer buffer)
        {
            return buffer.handle?.DangerousGetHandle() ?? IntPtr.Zero;
        }

        public static implicit operator SafeHandle(NativeBuffer buffer)
        {
            // Marshalling code will throw on null for SafeHandle
            return buffer.handle ?? EmptyHandle;
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
                return BytePointer[index];
            }
            set
            {
                if (index < 0 || index >= this.Capacity) throw new ArgumentOutOfRangeException();
                BytePointer[index] = value;
            }
        }

        private unsafe void Resize(long size)
        {
            Debug.Assert(size >= 0);

            if (size == 0)
            {
                this.ReleaseHandle();
                return;
            }

            if (this.handle == null)
            {
                this.handle = HeapHandleCache.Instance.Acquire((uint)size);
            }
            else
            {
                this.handle.Resize((UIntPtr)size);
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

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.ReleaseHandle();
        }

        private class EmptySafeHandle : SafeHandle
        {
            public EmptySafeHandle() : base(IntPtr.Zero, true) { }

            public override bool IsInvalid
            {
                get { return true; }
            }

            protected override bool ReleaseHandle()
            {
                return true;
            }
        }
    }
}
