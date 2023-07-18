// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using XTask.Logging;

namespace XTask.Tests.Logging;

public class ConsoleLoggerTests
{
    public abstract class TestConsoleLogger : ConsoleLogger
    {
        public TestConsoleLogger()
            : base()
        {
        }

        protected override void WriteColor(ConsoleColor color, string value)
        {
            TestWriteColor(color, value);
        }

        public abstract void TestWriteColor(ConsoleColor color, string value);
    }

    [Theory,
        MemberData(nameof(ColorData))]
    public void ColorsAreAsExpected(WriteStyle style, ConsoleColor expected)
    {
        TestConsoleLogger logger = Substitute.ForPartsOf<TestConsoleLogger>();
        logger.Write(style, "Foo");
        logger.Received(1).TestWriteColor(expected, Arg.Any<string>());
    }

    public static IEnumerable<object[]> ColorData
    {
        get
        {
            return new[]
            {
                new object[] { WriteStyle.Current, Console.ForegroundColor },
                new object[] { WriteStyle.Critical, ConsoleColor.Red },
                new object[] { WriteStyle.Important, ConsoleColor.Yellow },
                new object[] { WriteStyle.Error | WriteStyle.Important, ConsoleColor.Red }
            };
        }
    }

    [Fact]
    public void UnderlineStyle()
    {
        TestConsoleLogger logger = Substitute.ForPartsOf<TestConsoleLogger>();
        logger.WriteLine(WriteStyle.Underline, "Foo");
        logger.Received(1).TestWriteColor(Arg.Any<ConsoleColor>(), "Foo\r\n---");
    }
}
