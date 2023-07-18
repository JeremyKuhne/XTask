// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using XTask.Logging;
using XTask.Tasks;

namespace XTask.Tests.Tasks;

public class HelpTaskTests
{
    [Fact]
    public void HelpLogsHelp()
    {
        ITaskRegistry registry = Substitute.For<ITaskRegistry>();
        HelpTask task = new(registry, "GeneralHelp");

        ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
        ILoggers loggers = Substitute.For<ILoggers>();
        interaction.Loggers.Returns(loggers);
        ILogger logger = Substitute.For<ILogger>();
        loggers[LoggerType.Result].Returns(logger);

        task.Execute(interaction).Should().Be(ExitCode.Success);
        logger.Received().WriteLine(WriteStyle.Fixed, "GeneralHelp");
    }
}
