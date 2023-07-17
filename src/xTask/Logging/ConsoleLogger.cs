// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using XTask.Systems.Console;
using XTask.Utility;

namespace XTask.Logging
{
    public class ConsoleLogger : TextTableLogger
    {
        private class ColorLookup : Tuple<WriteStyle, ConsoleColor>
        {
            public ColorLookup(WriteStyle writeStyle, ConsoleColor color)
                : base (writeStyle, color)
            {
            }

            public WriteStyle WriteStyle { get { return Item1; } }
            public ConsoleColor ConsoleColor { get { return Item2; } }
        }

        private static readonly ConsoleLogger s_Instance;

        private readonly ConsoleColor _baseColor;
        private readonly List<ColorLookup> _colorTable;

        protected ConsoleLogger()
        {
            // TODO: Allow setting from config
            _baseColor = Console.ForegroundColor;
            _colorTable = new List<ColorLookup>();

            // Error state first
            _colorTable.Add(new ColorLookup(WriteStyle.Critical | WriteStyle.Error, ConsoleColor.Red)); 
            _colorTable.Add(new ColorLookup(WriteStyle.Italic | WriteStyle.Bold | WriteStyle.Important, ConsoleColor.Yellow));
        }

        static ConsoleLogger()
        {
            s_Instance = new ConsoleLogger();
        }

        public static ILogger Instance
        {
            get
            {
                return s_Instance;
            }
        }

        protected override void WriteInternal(WriteStyle style, string value)
        {
            ConsoleColor color = _baseColor;
            foreach (var lookup in _colorTable)
            {
                if ((lookup.WriteStyle & style) != 0)
                {
                    color = lookup.ConsoleColor;
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
        {
            WriteColor(color, Strings.Underline(value));
        }

        protected virtual void WriteColor(ConsoleColor color, string value)
        {
            ConsoleHelper.Console.WriteLockedColor(color, value);
        }

        protected override int TableWidth
        {
            get { return Math.Max(80, Console.BufferWidth); }
        }
    }
}