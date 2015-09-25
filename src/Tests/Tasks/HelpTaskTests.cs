// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Tasks
{
    using FluentAssertions;
    using NSubstitute;
    using XTask.Logging;
    using XTask.Tasks;
    using XTask.Utility;
    using Xunit;

    public class HelpTaskTests
    {
        [Fact]
        public void HelpLogsHelp()
        {
            ITaskRegistry registry = Substitute.For<ITaskRegistry>();
            HelpTask task = new HelpTask(registry, "GeneralHelp");

            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Result].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.Success);
            logger.Received().WriteLine(WriteStyle.Fixed, "GeneralHelp");
        }
    }
}
