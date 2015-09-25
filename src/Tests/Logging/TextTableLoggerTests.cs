// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Logging
{
    using System.Text;
    using FluentAssertions;
    using XTask.Logging;
    using XTask.Utility;
    using Xunit;

    public class TextTableLoggerTests
    {
        private class TextTableLoggerTester : TextTableLogger
        {
            private int tableWidth;

            public TextTableLoggerTester(int tableWidth)
            {
                this.tableWidth = tableWidth;
                this.Output = new StringBuilder();
            }

            protected override int TableWidth { get { return this.tableWidth; } }
            public void SetTableWidth(int width) { this.tableWidth = width; }
            public StringBuilder Output { get; private set; }

            protected override void WriteInternal(WriteStyle style, string value)
            {
                if (style.HasFlag(WriteStyle.Underline))
                {
                    this.Output.Append(Strings.Underline(value));
                }
                else
                {
                    this.Output.Append(value);
                }
            }
        }

        [Fact]
        public void SimpleOutputNoHeader()
        {
            Table table = Table.Create(ColumnFormat.FromCount(3));
            table.HasHeader = false;
            table.AddRow("One", "Two", "Three");
            table.AddRow("Four", "Five", "Six");

            TextTableLoggerTester logger = new TextTableLoggerTester(40);
            logger.Write(table);
            logger.Output.ToString().Should().Be("One   Two   Three\r\nFour  Five  Six\r\n");
        }

        [Fact]
        public void MinRoomNoHeader()
        {
            Table table = Table.Create(ColumnFormat.FromCount(3));
            table.HasHeader = false;
            table.AddRow("One", "Two", "Three");
            table.AddRow("Four", "Five", "Six");

            TextTableLoggerTester logger = new TextTableLoggerTester(6);
            logger.Write(table);
            logger.Output.ToString().Should().Be("O T T\r\nF F S\r\n");
        }

        [Fact]
        public void SimpleOutputHeader()
        {
            Table table = Table.Create(ColumnFormat.FromCount(3));
            table.AddRow("One", "Two", "Three");
            table.AddRow("Four", "Five", "Six");

            TextTableLoggerTester logger = new TextTableLoggerTester(40);
            logger.Write(table);
            logger.Output.ToString().Should().Be("One   Two   Three \r\n----- ----- ------\r\nFour  Five  Six\r\n");
        }

        [Fact]
        public void MinRoomHeader()
        {
            Table table = Table.Create(ColumnFormat.FromCount(3));
            table.AddRow("One", "Two", "Three");
            table.AddRow("Four", "Five", "Six");

            TextTableLoggerTester logger = new TextTableLoggerTester(6);
            logger.Write(table);
            logger.Output.ToString().Should().Be("O T T\r\n- - -\r\nF F S\r\n");
        }

        [Fact]
        public void RequiredVisibilityLimitedSpace()
        {
            Table table = Table.Create(new ColumnFormat(3), new ColumnFormat(3), new ColumnFormat(3, visibility: ContentVisibility.ShowAll)); ;
            table.AddRow("One", "Two", "Now is the time for all good men to come to the aid of their country.");
            table.AddRow("Four", "Five", "Sixes");

            TextTableLoggerTester logger = new TextTableLoggerTester(20);
            logger.Write(table);
            logger.Output.ToString().Should().Be("One Two Now is the \r\n--- --- -----------\r\nFou Fiv Sixes\r\n");
        }

        [Fact]
        public void RequiredVisibility()
        {
            Table table = Table.Create(new ColumnFormat(3), new ColumnFormat(3), new ColumnFormat(3, visibility: ContentVisibility.ShowAll)); ;
            table.HasHeader = false;
            table.AddRow("One", "Two", "Now is the time for all good men to come to the aid of their country.");
            table.AddRow("Four", "Five", "Sixes");

            TextTableLoggerTester logger = new TextTableLoggerTester(120);
            logger.Write(table);
            logger.Output.ToString().Should().Be("One   Two   Now is the time for all good men to come to the aid of their country.\r\nFour  Five  Sixes\r\n");
        }

        [Fact]
        public void RequiredVisibilityGrabFreespace()
        {
            Table table = Table.Create(new ColumnFormat(1), new ColumnFormat(1), new ColumnFormat(6, visibility: ContentVisibility.ShowAll)); ;
            table.HasHeader = false;
            table.AddRow("One", "Two", "Now is the time for all good men.");
            table.AddRow("Now is the time for all good men.", "Five", "Sixes");

            TextTableLoggerTester logger = new TextTableLoggerTester(80);
            logger.Write(table);
            logger.Output.ToString().Should().Be("One                                Two   Now is the time for all good men.\r\nNow is the time for all good men.  Five  Sixes\r\n");
        }

        [Fact]
        public void CompressedWhitespace()
        {
            Table table = Table.Create(new ColumnFormat(3, visibility: ContentVisibility.CompressWhitespace)); ;
            table.HasHeader = false;
            table.AddRow("Foo\t     \t\t\t\r\n\t\t\t- Things\r\n\tThat");
            table.AddRow("Four");

            TextTableLoggerTester logger = new TextTableLoggerTester(40);
            logger.Write(table);
            logger.Output.ToString().Should().Be("Foo - Things That\r\nFour\r\n");
        }
    }
}
