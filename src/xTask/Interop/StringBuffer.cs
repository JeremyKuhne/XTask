// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Native buffer that deals in char size increments. Dispose to free memory. Allows buffers larger
    /// than a maximum size string to enable working with very large string arrays.
    /// 
    /// A more performant replacement for StringBuilder when performing native interop.
    /// </summary>
    /// <remarks>
    /// Suggested use through P/Invoke: define DllImport arguments that take a character buffer as IntPtr.
    /// NativeStringBuffer has an implicit conversion to IntPtr.
    /// </remarks>
    public class StringBuffer : NativeBuffer
    {
        private ulong length;

        /// <summary>
        /// Instantiate the buffer with capacity for at least the specified number of characters. Capacity
        /// includes the trailing null character.
        /// </summary>
        public StringBuffer(ulong initialCapacity = 0)
            : base(initialCapacity)
        {
        }

        /// <summary>
        /// Instantiate the buffer with a copy of the specified string.
        /// </summary>
        public unsafe StringBuffer(string initialContents)
            : base(0)
        {
            // We don't pass the count of bytes to the base constructor, appending will
            // initialize to the correct size for the specified initial contents.
            if (initialContents != null)
            {
                this.AppendInternal(initialContents, startIndex: 0, count: initialContents.Length);
            }
        }

        /// <summary>
        /// Get/set the character at the given index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to index outside of the buffer length.</exception>
        public new unsafe char this[ulong index]
        {
            get
            {
                if (index >= this.Length) throw new ArgumentOutOfRangeException(nameof(index));
                return CharPointer[index];
            }
            set
            {
                if (index >= this.Length) throw new ArgumentOutOfRangeException(nameof(index));
                CharPointer[index] = value;
            }
        }

        /// <summary>
        /// Character capacity of the buffer. Includes the count for the trailing null character.
        /// </summary>
        public override ulong Capacity
        {
            get
            {
                ulong byteCapacity = base.Capacity;
                return byteCapacity == 0 ? 0 : byteCapacity / sizeof(char);
            }
        }

        /// <summary>
        /// Ensure capacity in characters is at least the given minimum.
        /// </summary>
        /// <exception cref="OutOfMemoryException">Thrown if unable to allocate memory when setting.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set <paramref name="nameof(Capacity)"/> to a value that is larger than the maximum addressable memory.</exception>
        public override void EnsureCapacity(ulong minCapacity)
        {
            if (minCapacity > (ulong.MaxValue / sizeof(char))) throw new ArgumentOutOfRangeException(nameof(minCapacity));
            base.EnsureCapacity(minCapacity * sizeof(char));
        }

        /// <summary>
        /// The logical length of the buffer in characters. (Does not include the final null.) Will automatically attempt to increase capacity.
        /// This is where the usable data ends.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set <paramref name="nameof(Length)"/> to a value that is larger than the maximum addressable memory.</exception>
        /// <exception cref="OutOfMemoryException">Thrown if unable to allocate memory when setting.</exception>
        public unsafe ulong Length
        {
            get { return this.length; }
            set
            {
                // Null terminate
                this.EnsureCapacity(value + 1);
                CharPointer[value] = '\0';

                this.length = value;
            }
        }

        /// <summary>
        /// For use when the native api null terminates but doesn't return a length.
        /// If no null is found, the length will not be changed.
        /// </summary>
        public unsafe void SetLengthToFirstNull()
        {
            char* buffer = CharPointer;
            ulong capacity = Capacity;
            for (ulong i = 0; i < capacity; i++)
            {
                if (buffer[i] == '\0')
                {
                    this.length = i;
                    break;
                }
            }
        }

        private unsafe char* CharPointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (char*)VoidPointer;
            }
        }

        /// <summary>
        /// Returns true if the buffer starts with the given string.
        /// </summary>
        public bool StartsWithOrdinal(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (this.Length < (ulong)value.Length) return false;
            return this.SubStringEquals(value, startIndex: 0, count: value.Length);
        }

        /// <summary>
        /// Returns true if the specified StringBuffer substring equals the given value.
        /// </summary>
        /// <param name="value">The value to compare against the specified substring.</param>
        /// <param name="startIndex">Start index of the sub string.</param>
        /// <param name="count">Length of the substring, or -1 to check all remaining.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="nameof(startIndex)"/> or <paramref name="nameof(count)"/> are outside the range
        /// of the buffer's length.
        /// </exception>
        public unsafe bool SubStringEquals(string value, ulong startIndex = 0, int count = -1)
        {
            if (value == null) return false;
            if (count < -1) throw new ArgumentOutOfRangeException(nameof(count));
            ulong realCount = count == -1 ? this.length - startIndex : (ulong)count;
            if (startIndex + realCount > this.length) throw new ArgumentOutOfRangeException(nameof(count));

            int length = value.Length;

            // Check the substring length against the input length
            if (realCount != (ulong)length) return false;

            fixed (char* valueStart = value)
            {
                char* bufferStart = CharPointer + startIndex;
                for (int i = 0; i < length; i++)
                {
                    if (*bufferStart++ != valueStart[i]) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Append the given string.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <param name="startIndex">The index in the input string to start appending from.</param>
        /// <param name="count">The count of characters to copy from the input string, or -1 for all remaining.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nameof(value)"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="nameof(startIndex)"/> or <paramref name="nameof(count)"/> are outside the range
        /// of <paramref name="nameof(value)"/> characters.
        /// </exception>
        public void Append(string value, int startIndex = 0, int count = -1)
        {
            if (value == null) throw new ArgumentNullException(value);
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count == -1) count = value.Length - startIndex;
            if (count < 0 || startIndex + count > value.Length) throw new ArgumentOutOfRangeException(nameof(count));

            this.AppendInternal(value, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void AppendInternal(string value, int startIndex, int count)
        {
            if (count == 0) return;

            ulong oldLength = this.Length;
            this.Length += (ulong)count;

            fixed (char* content = value)
            {
                Buffer.MemoryCopy(
                    source: content + startIndex,
                    destination: CharPointer + oldLength,
                    destinationSizeInBytes: (long)base.Capacity,
                    sourceBytesToCopy: count * sizeof(char));
            }
        }

        /// <summary>
        /// Split the contents into strings via the given split characters.
        /// </summary>
        /// <exception cref="OverflowException">Thrown if the substring is too big to fit in a string.</exception>
        unsafe public IEnumerable<string> Split(char splitCharacter)
        {
            var strings = new List<string>();
            char* start = CharPointer;
            char* current = start;

            ulong length = this.Length;

            for (ulong i = 0; i < length; i++)
            {
                if (splitCharacter == *current)
                {
                    // Split
                    strings.Add(new string(value: start, startIndex: 0, length: checked((int)(current - start))));
                    start = current + 1;
                }

                current++;
            }

            strings.Add(new string(value: start, startIndex: 0, length: checked((int)(current - start))));

            return strings;
        }

        /// <summary>
        /// Split the contents into strings via the given split characters.
        /// </summary>
        /// <param name="splitCharacters">Characters to split on, or null/empty to split on whitespace.</param>
        /// <exception cref="OverflowException">Thrown if the substring is too big to fit in a string.</exception>
        unsafe public IEnumerable<string> Split(params char[] splitCharacters)
        {
            bool splitWhite = splitCharacters == null || splitCharacters.Length == 0;

            var strings = new List<string>();
            char* start = CharPointer;
            char* current = start;

            ulong length = this.Length;

            for (ulong i = 0; i < length; i++)
            {
                if ((splitWhite && Char.IsWhiteSpace(*current))
                 || (!splitWhite && ContainsChar(splitCharacters, *current)))
                {
                    // Split
                    strings.Add(new string(value: start, startIndex: 0, length: checked((int)(current - start))));
                    start = current + 1;
                }

                current++;
            }

            strings.Add(new string(value: start, startIndex: 0, length: checked((int)(current - start))));

            return strings;
        }

        /// <summary>
        /// True if the buffer contains the given character.
        /// </summary>
        public unsafe bool Contains(char value)
        {
            char* start = CharPointer;
            ulong length = this.Length;

            for (ulong i = 0; i < length; i++)
            {
                if (*start++ == value) return true;
            }

            return false;
        }

        /// <summary>
        /// True if the buffer contains any of the specified characters.
        /// </summary>
        public unsafe bool Contains(params char[] values)
        {
            if (values == null || values.Length == 0) return false;

            char* start = CharPointer;
            ulong length = this.Length;

            for (ulong i = 0; i < length; i++)
            {
                if (ContainsChar(values, *start)) return true;
                start++;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsChar(char[] source, char value)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (value == source[i]) return true;
            }
            return false;
        }

        /// <summary>
        /// String representation of the entire buffer. If the buffer is larger than the maximum size string (int.MaxValue) will truncate.
        /// </summary>
        public unsafe override string ToString()
        {
            if (this.Length == 0) return String.Empty;
            return new string(CharPointer, startIndex: 0, length: this.length > int.MaxValue ? int.MaxValue : checked((int)this.Length));
        }

        /// <summary>
        /// Get the given substring in the buffer.
        /// </summary>
        /// <param name="count">Count of characters to take, or remaining characters from <paramref name="nameof(startIndex)"/> if -1.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="nameof(startIndex)"/> or <paramref name="nameof(count)"/> are outside the range of the buffer's length
        /// or count is greater than the maximum string size (int.MaxValue).
        /// </exception>
        public unsafe string SubString(ulong startIndex, int count = -1)
        {
            if (this.Length > 0 && startIndex > this.Length - 1) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < -1) throw new ArgumentOutOfRangeException(nameof(count));

            ulong realCount = count == -1 ? this.length - startIndex : (ulong)count;
            if (realCount > int.MaxValue || startIndex + realCount > this.Length) throw new ArgumentOutOfRangeException(nameof(count));
            if (realCount == 0) return String.Empty;

            return new string(value: CharPointer + startIndex, startIndex: 0, length: checked((int)realCount));
        }
    }
}
