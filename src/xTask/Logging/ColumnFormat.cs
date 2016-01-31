// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using System.Linq;

    public struct ColumnFormat
    {
        public int Width { get; set; }
        public Justification Justification { get; set; }
        public ContentVisibility Visibility { get; set; }

        public ColumnFormat(int width, ContentVisibility visibility = ContentVisibility.Default, Justification justification = Justification.Left)
        {
            Width = width == 0 ? 1 : Math.Abs(width);
            Justification = justification;
            Visibility = visibility;
        }

        /// <summary>
        /// Construct a simple format array of relative column widths, forcing the first to be visible if possible
        /// </summary>
        public static ColumnFormat[] FromWidths(params int[] widths)
        {
            return Helper(firstColumnMustBeVisible: true, widths: widths);
        }

        /// <summary>
        /// Construct a simple format array of x columns of equivalent width, forcing the first to be visible if possible
        /// </summary>
        public static ColumnFormat[] FromCount(int columns)
        {
            return Helper(firstColumnMustBeVisible: true, widths: new int[columns]);
        }

        private static ColumnFormat[] Helper(bool firstColumnMustBeVisible, params int[] widths)
        {
            ColumnFormat[] formats = new ColumnFormat[widths.Length];
            for (int i = 0; i < widths.Length; ++i)
            {
                formats[i] = new ColumnFormat(widths[i]);
            }

            if (firstColumnMustBeVisible) formats[0].Visibility = ContentVisibility.ShowAll;

            return formats;
        }

        /// <summary>
        /// Scale the widths given in the column specification to split up the specified full width as evenly as possible
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the width is smaller than the number of columns</exception>
        public static int[] ScaleColumnWidths(int fullWidth, ColumnFormat[] columnFormats)
        {
            fullWidth = Math.Abs(fullWidth);

            // Two small to fit anything in
            if (fullWidth < columnFormats.Length) { throw new ArgumentOutOfRangeException(nameof(fullWidth)); }

            int[] specifiedWidths = columnFormats.Select(c => c.Width).ToArray();
            return ScaleWidths(fullWidth, specifiedWidths);
        }

        private static int[] ScaleWidths(int fullWidth, int[] widths)
        {
            int[] columnWidths = new int[widths.Length];
            int remainingWidth = fullWidth;

            int fractionalWidths = 0;

            // Add up the specified widths
            for (int i = 0; i < columnWidths.Length; i++)
            {
                fractionalWidths += (columnWidths[i] = widths[i]);
            }

            // Scale up to whole numbers, adding any remainder
            bool zeroSpace = false;
            double widthMultiplier = (double)fullWidth / fractionalWidths;
            for (int i = 0; i < columnWidths.Length; i++)
            {
                int scaledWidth = (int)(columnWidths[i] * widthMultiplier);
                if (scaledWidth == 0)
                {
                    zeroSpace = true;
                    columnWidths[i] = 1;
                }
                else
                {
                    columnWidths[i] = scaledWidth;
                }

                remainingWidth -= columnWidths[i];
            }

            if (remainingWidth > 0) columnWidths[columnWidths.Length - 1] += remainingWidth;

            if (zeroSpace)
            {
                // Recursing should continue to drop us down to a truer fit
                return ColumnFormat.ScaleWidths(fullWidth, columnWidths);
            }
            else
            {
                return columnWidths;
            }
        }
    }
}
