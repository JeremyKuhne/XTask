﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using XTask.Logging;

namespace XTask.Tests.Logging;

public class AggregatedLoggerTests
{
    public abstract class TestLogger : Logger
    {
        protected override void WriteInternal(WriteStyle style, string value)
        {
            TestWriteInternal(style, value);
        }

        public void TestWriteInternal(WriteStyle style, string value)
        {
        }
    }

    [Fact]
    public void NullConstructorShouldThrow()
    {
        Action action = () => new AggregatedLogger(null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EmptyArrayShouldNotThow()
    {
        AggregatedLogger logger = new(new ILogger[0]);
        logger.Write("Foo");
    }

    [Fact]
    public void AllLoggersShouldLog()
    {
        TestLogger loggerOne = Substitute.ForPartsOf<TestLogger>();
        TestLogger loggerTwo = Substitute.ForPartsOf<TestLogger>();
        AggregatedLogger logger = new(loggerOne, loggerTwo);

        logger.Write("Foo");
        ITable table = Table.Create(1);
        logger.Write(table);

        loggerOne.Received(1).TestWriteInternal(WriteStyle.Current, "Foo");
        loggerTwo.Received(1).TestWriteInternal(WriteStyle.Current, "Foo");
        loggerOne.Received(1).Write(table);
        loggerTwo.Received(1).Write(table);
    }
}
