// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Logging
{
    using System;
    using FluentAssertions;
    using NSubstitute;
    using XTask.Logging;
    using Xunit;

    public class AggregatedLoggerTests
    {
        public abstract class TestLogger : Logger
        {
            protected override void WriteInternal(WriteStyle style, string value)
            {
                this.TestWriteInternal(style, value);
            }

            public void TestWriteInternal(WriteStyle style, string value)
            {
            }
        }

        [Fact]
        public void NullConstructorShouldThrow()
        {
            Action action = () => new AggregatedLogger(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void EmptyArrayShouldNotThow()
        {
            AggregatedLogger logger = new AggregatedLogger(new ILogger[0]);
            logger.Write("Foo");
        }

        [Fact]
        public void AllLoggersShouldLog()
        {
            TestLogger loggerOne = Substitute.ForPartsOf<TestLogger>();
            TestLogger loggerTwo = Substitute.ForPartsOf<TestLogger>();
            AggregatedLogger logger = new AggregatedLogger(loggerOne, loggerTwo);

            logger.Write("Foo");
            ITable table = Table.Create(1);
            logger.Write(table);

            loggerOne.Received(1).TestWriteInternal(WriteStyle.Current, "Foo");
            loggerTwo.Received(1).TestWriteInternal(WriteStyle.Current, "Foo");
            loggerOne.Received(1).Write(table);
            loggerTwo.Received(1).Write(table);
        }
    }
}
