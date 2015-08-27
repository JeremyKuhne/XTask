// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using XTask.ConsoleSystem;
    using XTask.Utility;

    public class ConsoleLogger : TextTableLogger
    {
        private class ColorLookup : Tuple<WriteStyle, ConsoleColor>
        {
            public ColorLookup(WriteStyle writeStyle, ConsoleColor color)
                : base (writeStyle, color)
            {
            }

            public WriteStyle WriteStyle { get { return this.Item1; } }
            public ConsoleColor ConsoleColor { get { return this.Item2; } }
        }

        private static ConsoleLogger instance;

        private ConsoleColor baseColor;
        private List<ColorLookup> colorTable;

        protected ConsoleLogger()
        {
            // TODO: Allow setting from config
            this.baseColor = Console.ForegroundColor;
            this.colorTable = new List<ColorLookup>();

            // Error state first
            this.colorTable.Add(new ColorLookup(WriteStyle.Critical | WriteStyle.Error, ConsoleColor.Red)); 
            this.colorTable.Add(new ColorLookup(WriteStyle.Italic | WriteStyle.Bold | WriteStyle.Important, ConsoleColor.Yellow));
        }

        static ConsoleLogger()
        {
            ConsoleLogger.instance = new ConsoleLogger();
        }

        public static ILogger Instance
        {
            get
            {
                return ConsoleLogger.instance;
            }
        }

        protected override void WriteInternal(WriteStyle style, string value)
        {
            ConsoleColor color = this.baseColor;
            foreach (var lookup in this.colorTable)
            {
                if ((lookup.WriteStyle & style) != 0)
                {
                    color = lookup.ConsoleColor;
                    break;
                }
            }

            if (style.HasFlag(WriteStyle.Underline))
            {
                this.WriteColorUnderlined(color, value);
            }
            else
            {
                this.WriteColor(color, value);
            }
        }

        private void WriteColorUnderlined(ConsoleColor color, string value)
        {
            this.WriteColor(color, Strings.Underline(value));
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