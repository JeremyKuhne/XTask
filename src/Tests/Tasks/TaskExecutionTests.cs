// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using System;
using XTask.Tasks;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using XTask.Settings;
using XTask.Logging;

namespace XTask.Tests.Tasks
{
    public class TaskExecutionTests
    {
        [Fact]
        public void ExecutionGetsSpecifiedCommand()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            ITaskRegistry taskRegistry = Substitute.For<ITaskRegistry>();
            ITask task = Substitute.For<ITask>();

            arguments.Command.Returns("ExecutionGetsSpecifiedCommand");
            taskRegistry["ExecutionGetsSpecifiedCommand"].Returns(task);

            TaskExecution execution = Substitute.ForPartsOf<TaskExecution>(arguments, taskRegistry, null);
            execution.ExecuteTask();
            task.Received(1).Execute(Arg.Any<ITaskInteraction>());
        }

        [Fact]
        public void HelpOutputsUsage()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            ITaskRegistry taskRegistry = Substitute.For<ITaskRegistry>();
            ITaskDocumentation docs = Substitute.For<ITaskDocumentation>();
            ITask task = Substitute.For<ITask>();
            task.GetService<ITaskDocumentation>().Returns(docs);

            arguments.Command.Returns("HelpOutputsUsage");
            arguments.HelpRequested.Returns(true);
            taskRegistry["HelpOutputsUsage"].Returns(task);

            TaskExecution execution = Substitute.ForPartsOf<TaskExecution>(arguments, taskRegistry, null);
            execution.ExecuteTask();
            docs.Received(1).GetUsage(Arg.Any<ITaskInteraction>());
        }

        public class TestTask : ITask, IDisposable
        {
            public int DisposeCount;

            public virtual void Dispose()
            {
                DisposeCount++;
            }

            public T GetService<T>() where T : class
            {
                return null;
            }
        }

        public class TestArgumentProvider : IArgumentProvider
        {
            public string Command { get; set; }

            public bool HelpRequested { get; set; }

            public IReadOnlyDictionary<string, string> Options { get; set; }

            public string Target { get; set; }

            public string[] Targets
            {
                get
                {
                    return new string[0];
                }
            }

            public T GetOption<T>(params string[] optionNames)
            {
                return default;
            }
        }

        // Something evil is going on here with NSubstitute when running all tests- see the history for what should work,
        // but only works when run by itself. Something to do with indexers?
        [Fact]
        public void TaskIsDisposed()
        {
            TestArgumentProvider arguments = new();
            arguments.Command = "TaskIsDisposed";

            SimpleTaskRegistry taskRegistry = new();
            TestTask task = new();
            taskRegistry.RegisterTask(() => task, "TaskIsDisposed");

            TaskExecution execution = Substitute.ForPartsOf<TaskExecution>(arguments, taskRegistry, null);
            execution.ExecuteTask();
            task.DisposeCount.Should().Be(1);
        }

        public class TestTaskExecution : TaskExecution
        {
            private readonly ITaskInteraction interaction;

            public TestTaskExecution(IArgumentProvider argumentProvider, ITaskRegistry taskRegistry, ITaskInteraction interaction)
                : base(argumentProvider, taskRegistry, null)
            {
                this.interaction = interaction;
            }

            protected override ITaskInteraction GetInteraction(ITask task)
            {
                return interaction;
            }
        }

        [Fact]
        public void InteractionIsDisposed()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            ITaskRegistry taskRegistry = Substitute.For<ITaskRegistry>();
            TestTaskInteraction interaction = new() { Arguments = arguments };

            TestTaskExecution execution = new(arguments, taskRegistry, interaction);
            execution.ExecuteTask();
            interaction.DisposeCount.Should().Be(1);
        }

        public class TestTaskInteraction : ITaskInteraction, IDisposable
        {
            public int DisposeCount;

            public virtual void Dispose()
            {
                DisposeCount++;
            }

            public IArgumentProvider Arguments { get; set; }

            public ILoggers Loggers { get; set; }

            public T GetService<T>() where T : class
            {
                return null;
            }

            public void Output(object value)
            {
            }
        }
    }
}
