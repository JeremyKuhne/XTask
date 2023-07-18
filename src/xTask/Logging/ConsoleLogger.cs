// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Systems.Console;
using XTask.Utility;

namespace XTask.Logging;

public class ConsoleLogger : TextTableLogger
{
    private readonly ConsoleColor _baseColor;
    private readonly List<(WriteStyle Style, ConsoleColor Color)> _colorTable;

    protected ConsoleLogger()
    {
        // TODO: Allow setting from config
        _baseColor = Console.ForegroundColor;
        _colorTable = new()
        {
            // Error state first
            new(WriteStyle.Critical | WriteStyle.Error, ConsoleColor.Red),
            new(WriteStyle.Italic | WriteStyle.Bold | WriteStyle.Important, ConsoleColor.Yellow)
        };
    }


    public static ILogger Instance { get; } = new ConsoleLogger();

    protected override void WriteInternal(WriteStyle style, string value)
    {
        ConsoleColor color = _baseColor;
        foreach (var (Style, Color) in _colorTable)
        {
            if ((Style & style) != 0)
            {
                color = Color;
                break;
            }
        }

        if (style.HasFlag(WriteStyle.Underline))
        {
            WriteColorUnderlined(color, value);
        }
        else
        {
            WriteColor(color, value);
        }
    }

    private void WriteColorUnderlined(ConsoleColor color, string value)
        => WriteColor(color, Strings.Underline(value));

    protected virtual void WriteColor(ConsoleColor color, string value)
        => ConsoleHelper.Console.WriteLockedColor(color, value);

    protected override int TableWidth => Math.Max(80, Console.BufferWidth);
}