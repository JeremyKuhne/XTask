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

    public class HelpTaskTests
    {
        [Fact]
        public void HelpLogsHelp()
        {
            HelpTask task = new HelpTask("GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            ILoggers loggers = Substitute.For<ILoggers>();
            interaction.Loggers.Returns(loggers);
            ILogger logger = Substitute.For<ILogger>();
            loggers[LoggerType.Result].Returns(logger);

            task.Execute(interaction).Should().Be(ExitCode.Success);
            logger.Received(1).WriteLine(WriteStyle.Fixed, "GeneralHelp");
        }
    }
}
