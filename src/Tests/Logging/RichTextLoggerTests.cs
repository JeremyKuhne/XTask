// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Logging;

namespace XTask.Tests.Logging;

public class RichTextLoggerTests
{
    private class TestRichTextLogger : RichTextLogger
    {
        public string RichTextBuffer
        {
            get { return _richText.ToString(); }
        }

        public string TestEscaping(string value)
        {
            return Escape(value);
        }
    }

    [Theory,
        InlineData("\\", "\\\\"),
        InlineData("{", "\\{"),
        InlineData("}", "\\}"),
        InlineData("愛", "\\u24859\\'3f"),
        InlineData("\t", "\\tab"),
        InlineData("\n", "\\par"),
        InlineData("\tAll for {愛}.", "\\tab All for \\{\\u24859\\'3f\\}.")]
    public void TestEscaping(string value, string expected)
    {
        TestRichTextLogger logger = new();
        logger.TestEscaping(value).Should().Be(expected);
    }

    [Theory,
        InlineData(WriteStyle.Bold, @"Foo", @"\b Foo\b0"),
        InlineData(WriteStyle.Underline, @"Foo", @"\ul Foo\ul0"),
        InlineData(WriteStyle.Italic, @"Foo", @"\i Foo\i0"),
        InlineData(WriteStyle.Fixed, @"Foo", @"\f1 Foo\f0"),
        InlineData(WriteStyle.Fixed | WriteStyle.Underline, @"Foo", @"\f1\ul Foo\f0\ul0")]
    public void TestFormatting(WriteStyle style, string value, string expected)
    {
        TestRichTextLogger logger = new();
        logger.Write(style, value);
        logger.RichTextBuffer.Should().Be(expected);
    }

    [Fact]
    public void SimpleTableOutput()
    {
        // Simple check that we match our expected output for various justifications and headers
        Table table = Table.Create(
            new ColumnFormat(1, ContentVisibility.CompressWhitespace, Justification.Left),
            new ColumnFormat(2, ContentVisibility.CompressWhitespace, Justification.Centered),
            new ColumnFormat(2, ContentVisibility.CompressWhitespace, Justification.Right));

        table.HasHeader = true;

        table.AddRow("Foo", "Bar", "FooBar");
        table.AddRow("one", "two", "three");

        RichTextLogger logger = new();
        logger.Write(table);
        string output = logger.ToString();
        output.Should().Be("""
            {\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\fhiminor\f0\fnil\fcharset0 Calibri;}{\f1\fmodern\fcharset0 Consolas;}}\uc1\pard\sl0\slmult1\fs22{\trowd\trgaph100\trrh-260\trpaddl100\trpaddr100\trpaddfl3\trpaddfr3\cellx1872\cellx5616\cellx9360\b\pard\intbl\widctlpar\ql Foo\cell\pard\intbl\widctlpar\qc Bar\cell\pard\intbl\widctlpar\qr FooBar\cell\b0\row}{\trowd\trgaph100\trrh-260\trpaddl100\trpaddr100\trpaddfl3\trpaddfr3\cellx1872\cellx5616\cellx9360\pard\intbl\widctlpar\ql one\cell\pard\intbl\widctlpar\qc two\cell\pard\intbl\widctlpar\qr three\cell\row}}
            """);
    }
}
