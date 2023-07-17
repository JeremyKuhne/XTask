// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XTask.Utility;

namespace XTask.Logging
{
    public abstract class TextTableLogger : Logger
    {
        protected abstract int TableWidth { get; }

        public override void Write(ITable table)
        {
            // Get the desired column widths in characters
            int columnCount = table.ColumnFormats.Length;
            int[] columnWidths = ColumnFormat.ScaleColumnWidths(TableWidth, table.ColumnFormats);

            // Convert tabs to spaces so we can layout properly and
            // get our max widths to see if we fall under the TableWidth
            List<string[]> rowsCopy = new();
            int[] maxColumnWidth = new int[columnCount];
            foreach (var row in table.Rows)
            {
                string[] rowCopy = new string[row.Length];
                for (int i = 0; i < row.Length; i++)
                {
                    rowCopy[i] = Strings.TabsToSpaces(row[i]);
                    if (table.ColumnFormats[i].Visibility.HasFlag(ContentVisibility.CompressWhitespace))
                        rowCopy[i] = Strings.CompressWhiteSpace(rowCopy[i]);
                    maxColumnWidth[i] = Math.Max(maxColumnWidth[i],
                        rowCopy[i].Length + 1); // We add a space between rows
                }
                rowsCopy.Add(rowCopy);
            }

            // Shrink the columns to fit, expanding the last column if needed with what we trim
            int availableSpace = 0;
            int neededSpace = 0;
            for (int i = 0; i < columnCount; ++i)
            {
                int overage = columnWidths[i] - maxColumnWidth[i];
                if (overage > 0)
                {
                    availableSpace += overage;
                    columnWidths[i] -= overage;
                }
                else
                {
                    neededSpace -= overage;
                }
            }

            // Try to fit columns that have required visibility (greedy)
            for (int i = 0; i < columnCount; ++i)
            {
                if (table.ColumnFormats[i].Visibility.HasFlag(ContentVisibility.ShowAll)
                    && maxColumnWidth[i] > columnWidths[i])
                {
                    int columnNeededSpace = maxColumnWidth[i] - columnWidths[i];
                    if (availableSpace >= columnNeededSpace)
                    {
                        // Plenty available
                        columnWidths[i] += columnNeededSpace;
                        availableSpace -= columnNeededSpace;
                        neededSpace -= columnNeededSpace;
                    }
                    else
                    {
                        // Take whatever is available
                        columnWidths[i] += availableSpace;
                        columnNeededSpace -= availableSpace;
                        neededSpace -= availableSpace;
                        availableSpace = 0;

                        // Keep culling a space from available columns while we can
                        while (columnNeededSpace > 0)
                        {
                            for (int j = 0; j < columnCount && columnNeededSpace > 0; j++)
                                if (!table.ColumnFormats[j].Visibility.HasFlag(ContentVisibility.ShowAll)
                                    && columnWidths[j] > 4) // Don't ever go below 4 (3 characters with a buffer space)
                                {
                                    columnWidths[j]--;
                                    columnWidths[i]++;
                                    availableSpace++;
                                    columnNeededSpace--;
                                    neededSpace--;
                                }

                            // If we were unable to find any more space, bail
                            if (availableSpace == 0)
                                break;
                            else
                                availableSpace = 0;
                        }
                    }
                }
            }

            // If we still have space and need it, try and fit any "normal" columns by evenly distributing space
            while (neededSpace > 0 && availableSpace > 0)
            {
                for (int i = 0; i < columnCount; ++i)
                {
                    if (maxColumnWidth[i] - columnWidths[i] > 0)
                    {
                        columnWidths[i]++;
                        neededSpace--;
                        if (--availableSpace == 0) break;
                    }
                }
            }

            // To make things look nicer, if we have enough space to add a single space to each column, do it
            if (availableSpace >= columnCount)
            {
                for (int i = 0; i < columnCount; ++i) columnWidths[i]++;
            }


            bool headerRow = table.HasHeader;

            StringBuilder rowBuilder = new(TableWidth + 1);
            foreach (var row in rowsCopy)
            {
                rowBuilder.Clear();
                for (int i = 0; i < row.Length; i++)
                {
                    bool lastColumn = (i == row.Length - 1);
                    rowBuilder.WriteColumn(
                        row[i],
                        table.ColumnFormats[i].Justification,
                        columnWidths[i] - 1,
                        lastColumn && !headerRow);

                    if (!lastColumn)
                    {
                        if (headerRow)
                            rowBuilder.Append('_');
                        else
                            rowBuilder.Append(' ');
                    }
                }

                if (headerRow)
                {
                    WriteLine(WriteStyle.Underline, rowBuilder.ToString());
                    headerRow = false;
                }
                else
                {
                    WriteLine(rowBuilder.ToString());
                }
            }
        }
    }
}
