// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using FluentAssertions;
using XTask.Logging;
using Xunit;

namespace XTask.Tests.Logging
{
    public class CsvLoggerTests
    {
        [Fact]
        public void StandardWriteDoesNotLog()
        {
            CsvLogger logger = new();
            logger.Write("Foo");
            logger.GetClipboardData().HasData.Should().BeFalse();
        }

        [Fact]
        public void TableLogs()
        {
            Table table = Table.Create(1, 1);
            table.AddRow("One", "Two");
            table.AddRow("Three", "Four");

            CsvLogger logger = new();
            logger.Write(table);
            ClipboardData data = logger.GetClipboardData();
            data.Format.Should().Be(ClipboardFormat.CommaSeparatedValues);
            Encoding.ASCII.GetString(data.ByteData.ToArray()).Should().Be("\"One\",\"Two\"\r\n\"Three\",\"Four\"\r\n");
        }
    }
}
