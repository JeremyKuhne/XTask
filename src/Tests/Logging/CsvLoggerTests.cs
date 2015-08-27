// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Logging
{
    using System.IO;
    using FluentAssertions;
    using XTask.Logging;
    using Xunit;

    public class CsvLoggerTests
    {
        [Fact]
        public void StandardWriteDoesNotLog()
        {
            CsvLogger logger = new CsvLogger();
            logger.Write("Foo");
            logger.GetClipboardData().Data.Should().BeNull();
        }

        [Fact]
        public void TableLogs()
        {
            Table table = Table.Create(1, 1);
            table.AddRow("One", "Two");
            table.AddRow("Three", "Four");

            CsvLogger logger = new CsvLogger();
            logger.Write(table);
            Stream stream = logger.GetClipboardData().Data as Stream;
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            reader.ReadToEnd().Should().Be("\"One\",\"Two\"\r\n\"Three\",\"Four\"\r\n");
        }
    }
}
