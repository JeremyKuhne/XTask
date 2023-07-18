// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace XTask.Logging;

/// <summary>
///  Simple logger that aggregates multiple ILoggers.
/// </summary>
public class AggregatedLogger : Logger
{
    private readonly IEnumerable<ILogger> _loggers;

    public AggregatedLogger(params ILogger[] loggers)
    {
        if (loggers is null) throw new ArgumentNullException(nameof(loggers));

        _loggers = loggers;
    }

    protected override void WriteInternal(WriteStyle style, string value)
    {
        foreach (ILogger logger in _loggers)
        {
            logger.Write(style, value);
        }
    }

    public override void Write(ITable table)
    {
        foreach (ILogger logger in _loggers)
        {
            logger.Write(table: table);
        }
    }
}