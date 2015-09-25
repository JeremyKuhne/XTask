// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Utility
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using NSubstitute;
    using XTask;
    using XTask.Systems.Console;
    using Xunit;

    public class ConsoleHelperTests
    {
        [Theory,
            MemberData("AnswerData")]
        public void AnswersReturnExpected(string answer, bool expected)
        {
            IConsoleService console = Substitute.For<IConsoleService>();
            console.ReadLine().Returns(answer);
            console.QueryYesNo(String.Empty).Should().Be(expected);
        }

        public static IEnumerable<object[]> AnswerData
        {
            get
            {
                return new[]
                {
                    new object[] { XTaskStrings.YesResponse, true },
                    new object[] { XTaskStrings.YesResponse.ToUpperInvariant(), true },
                    new object[] { XTaskStrings.YesShortResponse, true },
                    new object[] { "Yep", false },
                    new object[] { "No", false }
                };
            }
        }

        [Fact]
        public void QueryFormatsAsExpected()
        {
            IConsoleService console = Substitute.For<IConsoleService>();
            console.QueryYesNo("Foo{0}", "bar").Should().BeFalse();
            console.Received(1).Write("Foobar [yes/no] \n");
        }

        [Fact]
        public void WriteResetsColorOnException()
        {
            IConsoleService console = Substitute.For<IConsoleService>();
            console
                .When(x => x.Write(Arg.Any<string>()))
                .Do(x=> { throw new Exception(); });

            Action action = () => console.WriteLockedColor(ConsoleColor.Black, "Foo");
            action.ShouldThrow<Exception>();

            console.Received(1).ResetColor();
        }

        [Fact]
        public void WriteSetsColorAsExpected()
        {
            IConsoleService console = Substitute.For<IConsoleService>();
            console.WriteLockedColor(ConsoleColor.Blue, "Foo");
            console.Received(1).ForegroundColor = ConsoleColor.Blue;
            console.Received(1).Write("Foo");
            console.Received(1).ResetColor();
        }

        [Fact]
        public void AppendFormatsAsExpected()
        {
            IConsoleService console = Substitute.For<IConsoleService>();

            using (console.AppendConsoleTitle("Rocks {0}", "on"))
            {
                console.Received(1).Title = ": Rocks on";
            }
        }

        [Fact]
        public void TitleIsAppendedAsExpected()
        {
            IConsoleService console = Substitute.For<IConsoleService>();
            console.Title.Returns("MyTitle");

            using (console.AppendConsoleTitle("Rocks"))
            {
                console.Received(1).Title = "MyTitle: Rocks";
                console.ClearReceivedCalls();
            }

            console.Received(1).Title = "MyTitle";
        }
    }
}
