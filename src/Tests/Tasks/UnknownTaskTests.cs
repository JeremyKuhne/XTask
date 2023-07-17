// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NSubstitute;
using XTask.Logging;
using XTask.Settings;
using XTask.Tasks;
using Xunit;

namespace XTask.Tests.Tasks
{
    public class UnknownTaskTests
    {
        [Fact]
        public void UnknownTaskLogsHelp()
        {
            ITaskRegistry registry = Substitute.For<ITaskRegistry>();
            UnknownTask task = new(registry, "GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Result].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.InvalidArgument);
            logger.Received().WriteLine(WriteStyle.Fixed, "GeneralHelp");
        }

        [Fact]
        public void NoParameters()
        {
            ITaskRegistry registry = Substitute.For<ITaskRegistry>();
            UnknownTask task = new(registry, "GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Status].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.InvalidArgument);
            logger.Received().WriteLine(WriteStyle.Error, XTaskStrings.ErrorNoParametersSpecified);
        }

        [Fact]
        public void UnkownCommand()
        {
            ITaskRegistry registry = Substitute.For<ITaskRegistry>();
            UnknownTask task = new(registry, "GeneralHelp");
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
            logger.Received().WriteLine(WriteStyle.Error, XTaskStrings.UnknownCommand, commandName);
        }
    }
}
