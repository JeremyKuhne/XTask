// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging;

public interface ITable
{
    bool HasHeader { get; }
    ColumnFormat[] ColumnFormats { get; }
    IEnumerable<string[]> Rows { get; }
}
