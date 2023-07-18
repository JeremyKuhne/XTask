// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using XTask.Logging;
using Xunit;

namespace XTask.Tests.Logging;

public class HtmlLoggerTests
{
    private class TestHtmlLogger : HtmlLogger
    {
        public string HtmlTextBuffer
        {
            get { return _htmlText.ToString(); }
        }

        public void TestAppendFormatedString(WriteStyle style, string value)
        {
            AppendFormatedString(style, value);
        }

        public string ToClipboardString()
        {
            return FormatForClipboard(ToString());
        }
    }

    [Theory,
        InlineData(WriteStyle.Current, "<Foo>\n", @"&lt;Foo&gt;<br>"),
        InlineData(WriteStyle.Bold | WriteStyle.Italic, "Foo\n", @"<b><i>Foo<br></i></b>"),
        InlineData(WriteStyle.Bold, @"Foo", @"<b>Foo</b>"),
        InlineData(WriteStyle.Italic, @"Foo", @"<i>Foo</i>"),
        InlineData(WriteStyle.Critical, @"Foo", @"<strong>Foo</strong>"),
        InlineData(WriteStyle.Important, @"Foo", @"<em>Foo</em>"),
        InlineData(WriteStyle.Underline, @"Foo", @"<u>Foo</u>"),
        InlineData(WriteStyle.Fixed, @"Foo", @"<pre>Foo</pre>")]
    public void TestFormatting(WriteStyle style, string value, string expected)
    {
        TestHtmlLogger logger = new();
        logger.TestAppendFormatedString(style, value);
        logger.HtmlTextBuffer.Should().EndWith(expected);
    }

    [Fact]
    public void TestClipboardFormat()
    {
        const string expected =
            "Format:HTML Format\r\n" +
            "Version:1.0\r\n" +
            "StartHTML:0000000125\r\n" +
            "EndHTML:0000000454\r\n" +
            "StartFragment:0000000261\r\n" +
            "EndFragment:0000000422\r\n" +
            "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><HTML><HEAD><TITLE>From Clipboard</TITLE></HEAD><BODY><!--StartFragment-->" +
            "<div style='font-size:11.0pt;font-family:Calibri,sans-serif><span style='font-size:11.0pt;font-family:Calibri,sans-serif;white-space:pre'>TestString</span></div><!--EndFragment--></BODY></HTML>";
        TestHtmlLogger logger = new();
        logger.Write("TestString");
        logger.ToClipboardString().Should().Be(expected);
    }

    [Fact]
    public void CheckTableOutputStability()
    {
        // Just checking to make sure outputing a table doesn't crash
        Table testTable = Table.Create(
            new ColumnFormat(1, ContentVisibility.Default, Justification.Left),
            new ColumnFormat(1, ContentVisibility.Default, Justification.Right),
            new ColumnFormat(1, ContentVisibility.Default, Justification.Centered));

        testTable.AddRow("one", "two", "three");
        TestHtmlLogger logger = new();
        logger.Write(testTable);
    }
}
