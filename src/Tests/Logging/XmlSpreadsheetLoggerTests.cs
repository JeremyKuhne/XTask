// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Logging
{
    using System.IO;
    using FluentAssertions;
    using XTask.Logging;
    using Xunit;

    public class XmlSpreadsheetLoggerTests
    {
        [Fact]
        public void StandardWriteDoesNotLog()
        {
            XmlSpreadsheetLogger logger = new XmlSpreadsheetLogger();
            logger.Write("Foo");
            logger.GetClipboardData().Data.Should().BeNull();
        }

        [Fact]
        public void SimpleTableLogs()
        {
            Table table = Table.Create(1, 1);
            table.AddRow("One", "Two");
            table.AddRow("Three", "Four");

            XmlSpreadsheetLogger logger = new XmlSpreadsheetLogger();
            logger.Write(table);
            Stream stream = logger.GetClipboardData().Data as Stream;
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            reader.ReadToEnd().Should().Be("<?xml version='1.0' encoding='utf-8' standalone='yes'?>\r\n<?mso-application progid='Excel.Sheet'?>\r\n<Workbook xmlns='urn:schemas-microsoft-com:office:spreadsheet'\r\n xmlns:o='urn:schemas-microsoft-com:office:office'\r\n xmlns:x='urn:schemas-microsoft-com:office:excel'\r\n xmlns:ss='urn:schemas-microsoft-com:office:spreadsheet'\r\n xmlns:html='http://www.w3.org/TR/REC-html40'>\r\n <Worksheet ss:Name='XTaskSheet'>\r\n  <Table>\r\n   <Row>\r\n    <Cell><Data ss:Type='String'>One</Data></Cell>\r\n    <Cell><Data ss:Type='String'>Two</Data></Cell>\r\n   </Row>\r\n   <Row>\r\n    <Cell><Data ss:Type='String'>Three</Data></Cell>\r\n    <Cell><Data ss:Type='String'>Four</Data></Cell>\r\n   </Row>\r\n  </Table>\r\n </Worksheet>\r\n</Workbook>\r\n");
        }
    }
}
