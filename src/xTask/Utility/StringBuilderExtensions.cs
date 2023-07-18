// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XTask.Logging;

namespace XTask.Utility
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        ///  Append a substring from a string to a StringBuilder.
        /// </summary>
        /// <param name="startIndex">The starting index in the given string to start appending from.</param>
        /// <param name="length">Number of character to copy, or -1 to copy to the end.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nameof(value)"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="nameof(startIndex)"/> or <paramref name="nameof(length)"/> are out of bounds of <paramref name="nameof(value)"/>.
        /// </exception>
        public unsafe static void AppendSubstring(this StringBuilder builder, string value, int startIndex, int length = -1)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex >= value.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length < 0)
            {
                length = value.Length - startIndex;
            }
            else if (value.Length - startIndex > length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (length == 0)
            {
                return;
            }

            fixed(char* start = value)
            {
                builder.Append(start + startIndex, valueCount: length);
            }
        }

        /// <summary>
        ///  Returns true if the StringBuilder starts with the given string.
        /// </summary>
        public static bool StartsWithOrdinal(this StringBuilder builder, string value)
        {
            if (value is null || builder.Length < value.Length) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (builder[i] != value[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///  Trims leading and trailing whitespace.
        /// </summary>
        public static void Trim(this StringBuilder builder)
        {
            builder.TrimStart();
            builder.TrimEnd();
        }

        /// <summary>
        ///  Trims leading whitespace.
        /// </summary>
        public static void TrimStart(this StringBuilder builder)
        {
            int start;
            for (start = 0; start < builder.Length; start++)
            {
                if (!char.IsWhiteSpace(builder[start]))
                {
                    break;
                }
            }

            builder.Remove(0, start);
        }

        /// <summary>
        ///  Trims trailing whitespace.
        /// </summary>
        public static void TrimEnd(this StringBuilder builder)
        {
            int end;
            for (end = builder.Length - 1; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(builder[end]))
                {
                    break;
                }
            }

            builder.Length = end + 1;
        }

        /// <summary>
        ///  Splits the given StringBuilder into strings based on the specified split characters.
        /// </summary>
        public static IEnumerable<string> Split(this StringBuilder builder, params char[] splitCharacters)
        {
            var strings = new List<string>();
            if (builder.Length == 0 || splitCharacters is null || splitCharacters.Length == 0)
            {
                strings.Add(builder.ToString());
                return strings;
            }

            int stringStart = 0;
            for (int i = 0; i < builder.Length; i++)
            {
                char current = builder[i];
                if (splitCharacters.Contains(current))
                {
                    // Split
                    strings.Add(builder.ToString(stringStart, i - stringStart));
                    stringStart = i + 1;
                }
            }

            if (stringStart <= builder.Length)
            {
                strings.Add(builder.ToString(stringStart, builder.Length - stringStart));
            }

            return strings;
        }

        /// <summary>
        ///  Writes a column of the given width and justification
        /// </summary>
        /// <param name="width">Number of characters to use for this column</param>
        /// <param name="noRightPadding">By default we want to put spaces on the right out to the width</param>
        public static void WriteColumn(this StringBuilder builder, string value, Justification justification, int width, bool noRightPadding = false)
        {
            int length = value.Length;

            if (length >= width)
            {
                builder.Append(value, startIndex: 0, count: width);
                return;
            }

            if (noRightPadding && justification == Justification.Left)
            {
                builder.Append(value);
                return;
            }

            int leftSpacing = 0;
            int rightSpacing = 0;

            switch (justification)
            {
                case Justification.Centered:
                    leftSpacing = (width - length) / 2;
                    rightSpacing = width - length - leftSpacing;
                    break;
                case Justification.Left:
                    rightSpacing = width - length;
                    break;
                case Justification.Right:
                    leftSpacing = width - length;
                    break;
            }

            if (leftSpacing > 0)
            {
                builder.Append(' ', leftSpacing);
            }

            builder.Append(value);
            if (rightSpacing > 0 && !noRightPadding)
            {
                builder.Append(' ', rightSpacing);
            }
        }
    }
}
