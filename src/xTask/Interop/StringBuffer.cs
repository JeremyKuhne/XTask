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
    /// Native buffer that deals in char size increments. A more performant replacement for StringBuilder
    /// when performing native interop. Dispose to free memory.
    /// </summary>
    /// <remarks>
    /// Suggested use through P/Invoke: define DllImport arguments that take a character buffer as IntPtr.
    /// NativeStringBuffer has an implicit conversion to IntPtr.
    /// </remarks>
    public class StringBuffer : NativeBuffer
    {
        // Interoping with string means anything over int isn't usefule
        private int length;

        /// <summary>
        /// Instantiate the buffer with capacity for the specified number of characters. Capacity
        /// includes the trailing null character.
        /// </summary>
        public StringBuffer(int initialLength = 0)
            : base(0)
        {
            // We don't pass the count of bytes to the base constructor, setting capacity will
            // initialize to the correct size for the specified number of characters.
            this.Capacity = initialLength;
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
                this.AppendInternal(initialContents);
            }
        }

        /// <summary>
        /// Get/set the character at the given index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to index outside of the buffer length.</exception>
        public new unsafe char this[long index]
        {
            get
            {
                if (index < 0 || index >= this.Length) throw new ArgumentOutOfRangeException(nameof(index));
                return CharPointer[index];
            }
            set
            {
                if (index < 0 || index >= this.Length) throw new ArgumentOutOfRangeException(nameof(index));
                CharPointer[index] = value;
            }
        }

        /// <summary>
        /// Character capacity of the buffer. Includes the count for the trailing null character.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set <paramref name="nameof(Capacity)"/> to a negative value.</exception>
        public override long Capacity
        {
            get
            {
                long byteCapacity = base.Capacity;
                return byteCapacity == 0 ? 0 : byteCapacity / sizeof(char);
            }
            set
            {
                if (value < 0 || value > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(value));
                base.Capacity = value * sizeof(char);
            }
        }

        /// <summary>
        /// The logical length of the buffer in characters. (Does not include the final null.)
        /// This is where the usable data ends.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set <paramref name="nameof(Length)"/> to a negative value.</exception>
        public unsafe int Length
        {
            get { return this.length; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

                // Leave room for the null
                if (this.Capacity <= value)
                {
                    this.Capacity = value + 1;
                }

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
            long capacity = Capacity;
            for (int i = 0; i < capacity; i++)
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
            if (this.Length < value.Length) return false;
            return this.SubStringEquals(value, startIndex: 0, count: value.Length);
        }

        /// <summary>
        /// Returns true if the specified substring equals the given value.
        /// </summary>
        /// <param name="value">The value to compare against the specified substring.</param>
        /// <param name="startIndex">Start index of the sub string.</param>
        /// <param name="count">Length of the substring, or -1 to check all remaining.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="nameof(startIndex)"/> or <paramref name="nameof(count)"/> are outside the range
        /// of the buffer's length.
        /// </exception>
        public unsafe bool SubStringEquals(string value, int startIndex = 0, int count = -1)
        {
            if (value == null) return false;
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < -1 || startIndex + count > this.length) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == -1) count = (int)this.length - startIndex;

            int length = value.Length;

            // Check the substring length against the input length
            if (count != length) return false;

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
            if (count < -1 || startIndex + count > value.Length) throw new ArgumentOutOfRangeException(nameof(count));
            this.AppendInternal(value, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void AppendInternal(string value, int startIndex = 0, int count = -1)
        {
            if (count == 0) return;
            if (count < 0) count = value.Length - startIndex;
            this.Length += count;
            fixed (void* content = value)
            {
                Buffer.MemoryCopy(content, VoidPointer, base.Capacity, count * sizeof(char));
            }
        }

        /// <summary>
        /// Split the contents into strings via the given split characters.
        /// </summary>
        unsafe public IEnumerable<string> Split(char splitCharacter)
        {
            var strings = new List<string>();
            char* start = CharPointer;
            char* current = start;

            int stringStart = 0;
            int length = this.Length;

            for (int i = 0; i < length; i++)
            {
                if (splitCharacter == *current++)
                {
                    // Split
                    strings.Add(new string(start, stringStart, i - stringStart));
                    stringStart = i + 1;
                }
            }

            if (stringStart <= length)
            {
                strings.Add(new string(start, stringStart, length - stringStart));
            }

            return strings;
        }

        /// <summary>
        /// Split the contents into strings via the given split characters.
        /// </summary>
        /// <param name="splitCharacters">Characters to split on, or null/empty to split on whitespace.</param>
        unsafe public IEnumerable<string> Split(params char[] splitCharacters)
        {
            bool splitWhite = splitCharacters == null || splitCharacters.Length == 0;

            var strings = new List<string>();
            char* start = CharPointer;
            char* current = start;

            int stringStart = 0;
            int length = this.Length;

            for (int i = 0; i < length; i++)
            {
                if ((splitWhite && Char.IsWhiteSpace(*current))
                 || (!splitWhite && ContainsChar(splitCharacters, *current)))
                {
                    // Split
                    strings.Add(new string(start, stringStart, i - stringStart));
                    stringStart = i + 1;
                }

                current++;
            }

            if (stringStart <= length)
            {
                strings.Add(new string(start, stringStart, length - stringStart));
            }

            return strings;
        }

        /// <summary>
        /// True if the buffer contains the given character.
        /// </summary>
        public unsafe bool Contains(char value)
        {
            char* start = CharPointer;
            int length = this.Length;

            for (int i = 0; i < length; i++)
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
            int length = this.Length;

            for (int i = 0; i < length; i++)
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

        public unsafe override string ToString()
        {
            if (this.Length == 0) return String.Empty;
            return new string(CharPointer, startIndex: 0, length: (int)this.Length);
        }

        /// <summary>
        /// Get the given substring in the buffer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="nameof(startIndex)"/> or <paramref name="nameof(count)"/> are outside the range
        /// of the buffer's length.
        /// </exception>
        public unsafe string ToString(int startIndex, int count = -1)
        {
            if (startIndex < 0 || (this.Length > 0 && startIndex > this.Length - 1)) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < -1 || startIndex + count > this.Length) throw new ArgumentOutOfRangeException(nameof(count));
            if (count < 0) count = (int)(this.Length - startIndex);
            if (count == 0) return String.Empty;

            return new string(CharPointer, startIndex: startIndex, length: count);
        }
    }
}
