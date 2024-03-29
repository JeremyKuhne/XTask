﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using XTask.Logging;
using XTask.Settings;
using XTask.Tasks;

namespace XTask.Tests.Tasks;

public class ConsoleTaskInteractionTests
{
    [Fact]
    public void StandardLoggerIsConsole()
    {
        ITask task = Substitute.For<ITask>();
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();

        using ConsoleTaskInteraction interaction = (ConsoleTaskInteraction)ConsoleTaskInteraction.Create(task, arguments, null);
        interaction.Loggers[LoggerType.Status].Should().BeSameAs(ConsoleLogger.Instance);
        interaction.Loggers[LoggerType.Result].Should().BeSameAs(ConsoleLogger.Instance);
    }

    [Fact]
    public void ClipOptionShouldHaveClipLoggers()
    {
        ITask task = Substitute.For<ITask>();
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
        arguments.GetOption<bool?>(StandardOptions.Clipboard).Returns(true);


        using ConsoleTaskInteraction interaction = (ConsoleTaskInteraction)ConsoleTaskInteraction.Create(task, arguments, null);
        interaction.Loggers[LoggerType.Status].Should().BeSameAs(ConsoleLogger.Instance);
        interaction.Loggers[LoggerType.Result].Should().BeOfType<AggregatedLogger>();
    }

    [Fact]
    public void ClipTaskShouldHaveClipLoggers()
    {
        ITask task = Substitute.For<ITask>();
        task.GetOptionDefault<bool>(StandardOptions.Clipboard[0]).Returns(true);
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();

        using ConsoleTaskInteraction interaction = (ConsoleTaskInteraction)ConsoleTaskInteraction.Create(task, arguments, null);
        interaction.Loggers[LoggerType.Status].Should().BeSameAs(ConsoleLogger.Instance);
        interaction.Loggers[LoggerType.Result].Should().BeOfType<AggregatedLogger>();
    }

    [Fact]
    public void ClipTaskWithClipOffShouldNotHaveClipLoggers()
    {
        ITask task = Substitute.For<ITask>();
        task.GetOptionDefault<bool>(StandardOptions.Clipboard[0]).Returns(true);
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
        arguments.GetOption<bool?>(StandardOptions.Clipboard).Returns(false);

        using ConsoleTaskInteraction interaction = (ConsoleTaskInteraction)ConsoleTaskInteraction.Create(task, arguments, null);
        interaction.Loggers[LoggerType.Status].Should().BeSameAs(ConsoleLogger.Instance);
        interaction.Loggers[LoggerType.Result].Should().BeSameAs(ConsoleLogger.Instance);
    }
}
