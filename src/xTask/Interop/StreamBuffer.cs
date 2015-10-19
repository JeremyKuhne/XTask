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

    /// <summary>
    /// Stream wrapper for access to the native heap that allows for automatic growth when writing.
    /// Also provides implicit conversion to IntPtr for P/Invoke convenience.
    /// Dispose to free the memory. Try to use with using statements.
    /// </summary>
    public class StreamBuffer : Stream
    {
        private NativeBuffer buffer;
        private UnmanagedMemoryStream stream;
        private bool disposed;

        public StreamBuffer(uint initialLength = 0, uint initialCapacity = 0)
        {
            if (initialCapacity < initialLength) initialCapacity = initialLength;
            this.buffer = new NativeBuffer(initialCapacity);
            this.SetLength(initialLength);
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

        public static implicit operator IntPtr(StreamBuffer buffer)
        {
            return buffer.buffer;
        }

        public void EnsureLength(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (this.Length < value) this.SetLength(value);
        }

        public override void SetLength(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (value == this.Length) return;

            this.Resize(value);
            this.stream.SetLength(value);
        }

        private unsafe void Resize(long size)
        {
            Debug.Assert(size >= 0);

            if (this.stream != null && this.buffer.Capacity >= (ulong)size) return;
            this.buffer.EnsureCapacity((ulong)size);

            long oldLength = this.Length;
            long oldPosition = this.Position;
            this.stream?.Dispose();

            this.stream = new UnmanagedMemoryStream(
                pointer: (byte*)((IntPtr)this.buffer).ToPointer(),
                length: oldLength,
                capacity: size,
                access: FileAccess.ReadWrite);

            if (oldPosition <= size) { this.Position = oldPosition; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "stream")]
        protected override void Dispose(bool disposing)
        {
            disposed = true;
            this.buffer.Dispose();

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
                if (offset != 0) throw new ArgumentException();
                return 0;
            }

            return this.stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0) return;

            if (this.stream == null)
            {
                // Mimic UnmanagedMemoryStream with a 0 length buffer
                if (offset != 0) throw new ArgumentException();
            }

            this.EnsureLength(this.Length + count);
            this.stream.Write(buffer, offset, count);
        }
    }
}
