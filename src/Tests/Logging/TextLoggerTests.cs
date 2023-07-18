// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Logging;

namespace XTask.Tests.Logging;

public class TextLoggerTests
{
    [Fact]
    public void SimpleWrite()
    {
        TextLogger logger = new();
        logger.Write("Foo");
        logger.ToString().Should().Be("Foo");
    }

    [Fact]
    public void SimpleWriteClipboardData()
    {
        TextLogger logger = new();
        logger.Write("Foo");
        logger.GetClipboardData().CharData.ToString().Should().Be("Foo");
    }

    [Fact]
    public void SimpleUnderline()
    {
        TextLogger logger = new();
        logger.Write(WriteStyle.Underline, "Foo");
        logger.ToString().Should().Be("Foo\r\n---");
    }

    [Fact]
    public void NoLogNullClipboardData()
    {
        TextLogger logger = new();
        logger.GetClipboardData().HasData.Should().BeFalse();
    }
}
