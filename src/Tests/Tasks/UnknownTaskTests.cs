// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Tasks
{
    using System;
    using NSubstitute;
    using Utility;
    using XTask.Tasks;
    using Xunit;
    using FluentAssertions;
    using Logging;
    using Settings;

    public class UnknownTaskTests
    {
        [Fact]
        public void UnknownTaskLogsHelp()
        {
            UnknownTask task = new UnknownTask("GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Result].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.InvalidArgument);
            logger.Received(1).WriteLine(WriteStyle.Fixed, "GeneralHelp");
        }

        [Fact]
        public void NoParameters()
        {
            UnknownTask task = new UnknownTask("GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Status].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.InvalidArgument);
            logger.Received(1).WriteLine(WriteStyle.Error, XTaskStrings.ErrorNoParametersSpecified);
        }

        [Fact]
        public void UnkownCommand()
        {
            UnknownTask task = new UnknownTask("GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            string commandName = "Foo";
            interaction.Arguments.Returns(arguments);
            interaction.Arguments.Command.Returns(commandName);
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Status].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.InvalidArgument);
            logger.Received(1).WriteLine(WriteStyle.Error, XTaskStrings.UnknownCommand, commandName);
        }
    }
}
