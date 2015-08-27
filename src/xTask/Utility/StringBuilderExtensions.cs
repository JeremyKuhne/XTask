// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using XTask.Logging;

    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Returns true if the StringBuilder starts with the given string.
        /// </summary>
        public static bool StartsWithOrdinal(this StringBuilder builder, string value)
        {
            if (value == null || builder.Length < value.Length) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (builder[i] != value[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Trims leading and trailing whitespace.
        /// </summary>
        public static void Trim(this StringBuilder builder)
        {
            builder.TrimStart();
            builder.TrimEnd();
        }

        /// <summary>
        /// Trims leading whitespace.
        /// </summary>
        public static void TrimStart(this StringBuilder builder)
        {
            int start = 0;
            for (start = 0; start < builder.Length; start++)
            {
                if (!Char.IsWhiteSpace(builder[start])) break;
            }
            builder.Remove(0, start);
        }

        /// <summary>
        /// Trims trailing whitespace.
        /// </summary>
        public static void TrimEnd(this StringBuilder builder)
        {
            int end = builder.Length - 1;
            for (end = builder.Length - 1; end >= 0; end--)
            {
                if (!Char.IsWhiteSpace(builder[end])) break;
            }

            builder.Length = end + 1;
        }

        /// <summary>
        /// Splits the given StringBuilder into strings based on the specified split characters.
        /// </summary>
        public static IEnumerable<string> Split(this StringBuilder builder, params char[] splitCharacters)
        {
            var strings = new List<string>();
            if (builder.Length == 0 || splitCharacters == null || splitCharacters.Length == 0)
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
        /// Writes a column of the given width and justification
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

            if (leftSpacing > 0) builder.Append(' ', leftSpacing);
            builder.Append(value);
            if (rightSpacing > 0 && !noRightPadding) builder.Append(' ', rightSpacing);
        }
    }
}
