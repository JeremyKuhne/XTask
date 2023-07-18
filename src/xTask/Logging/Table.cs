// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace XTask.Logging;

/// <summary>
///  Basic ITable implementation.
/// </summary>
public class Table : ITable
{
    private readonly List<string[]> _rows = new();
    public IEnumerable<string[]> Rows => _rows;
    public ColumnFormat[] ColumnFormats { get; private set; }
    public bool HasHeader { get; set; }

    protected Table(params ColumnFormat[] rowFormats)
    {
        ColumnFormats = rowFormats;
        HasHeader = true;
    }

    /// <summary>
    ///  Creates a standard table with a header row.
    /// </summary>
    public static Table Create(params ColumnFormat[] rowFormats) => new(rowFormats: rowFormats);

    /// <summary>
    ///  Creates a standard table with a header row with the specifed widths (forcing first column visibility)
    /// </summary>
    public static Table Create(params int[] widths) => new(ColumnFormat.FromWidths(widths));

    /// <summary>.
    ///  Add values for a row's columns
    /// </summary>
    public void AddRow(params string[] values)
    {
        string[] row = new string[ColumnFormats.Length];
        for (int i = 0; i < row.Length; i++)
        {
            row[i] = i < values.Length ? (values[i] ?? string.Empty) : string.Empty;
        }
        _rows.Add(row);
    }
}
