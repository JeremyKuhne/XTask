// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Basic ITable implementation
    /// </summary>
    public class Table : ITable
    {
        private List<string[]> rows = new List<string[]>();
        public IEnumerable<string[]> Rows { get { return this.rows; } }
        public ColumnFormat[] ColumnFormats { get; private set; }
        public bool HasHeader { get; set; }

        protected Table(params ColumnFormat[] rowFormats)
        {
            this.ColumnFormats = rowFormats;
            this.HasHeader = true;
        }

        /// <summary>
        /// Creates a standard table with a header row
        /// </summary>
        public static Table Create(params ColumnFormat[] rowFormats)
        {
            return new Table(rowFormats: rowFormats);
        }

        /// <summary>
        /// Creates a standard table with a header row with the specifed widths (forcing first column visibility)
        /// </summary>
        public static Table Create(params int[] widths)
        {
            return new Table(ColumnFormat.FromWidths(widths));
        }

        /// <summary>
        /// Add values for a row's columns
        /// </summary>
        public void AddRow(params string[] values)
        {
            string[] row = new string[this.ColumnFormats.Length];
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = i < values.Length ? (values[i] ?? String.Empty) : String.Empty;
            }
            this.rows.Add(row);
        }
    }
}
