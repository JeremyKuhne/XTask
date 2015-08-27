// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Logging
{
    using FluentAssertions;
    using XTask.Logging;
    using Xunit;

    public class TextLoggerTests
    {
        [Fact]
        public void SimpleWrite()
        {
            TextLogger logger = new TextLogger();
            logger.Write("Foo");
            logger.ToString().Should().Be("Foo");
        }

        [Fact]
        public void SimpleWriteClipboardData()
        {
            TextLogger logger = new TextLogger();
            logger.Write("Foo");
            logger.GetClipboardData().Data.Should().Be("Foo");
        }

        [Fact]
        public void SimpleUnderline()
        {
            TextLogger logger = new TextLogger();
            logger.Write(WriteStyle.Underline, "Foo");
            logger.ToString().Should().Be("Foo\r\n---");
        }

        [Fact]
        public void NoLogNullClipboardData()
        {
            TextLogger logger = new TextLogger();
            logger.GetClipboardData().Data.Should().BeNull();
        }
    }
}
