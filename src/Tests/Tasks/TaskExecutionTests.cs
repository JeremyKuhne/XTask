// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Tasks
{
    using NSubstitute;
    using Services;
    using Settings;
    using System;
    using XTask.Tasks;
    using Xunit;

    public class TaskExecutionTests
    {
        [Fact]
        public void ExecutionGetsSpecifiedCommand()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            ITaskRegistry taskRegistry = Substitute.For<ITaskRegistry>();
            ITask task = Substitute.For<ITask>();

            arguments.Command.Returns("TestCommand");
            taskRegistry["TestCommand"].Returns(task);

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

            arguments.Command.Returns("TestCommand");
            arguments.HelpRequested.Returns(true);
            taskRegistry["TestCommand"].Returns(task);

            TaskExecution execution = Substitute.ForPartsOf<TaskExecution>(arguments, taskRegistry, null);
            execution.ExecuteTask();
            docs.Received(1).GetUsage(Arg.Any<ITaskInteraction>());
        }

        [Fact]
        public void TaskIsDisposed()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            ITaskRegistry taskRegistry = Substitute.For<ITaskRegistry>();
            ITask task = Substitute.For<ITask, IDisposable>();

            arguments.Command.Returns("TestCommand");
            taskRegistry["TestCommand"].Returns(task);

            TaskExecution execution = Substitute.ForPartsOf<TaskExecution>(arguments, taskRegistry, null);
            execution.ExecuteTask();
            ((IDisposable)task).Received(1).Dispose();
        }

        public abstract class TestTaskExecution : TaskExecution
        {
            public TestTaskExecution(IArgumentProvider argumentProvider, ITaskRegistry taskRegistry, ITypedServiceProvider services)
                : base(argumentProvider, taskRegistry, services)
            {
            }

            protected override ITaskInteraction GetInteraction(ITask task)
            {
                return this.TestGetInteraction(task);
            }

            public abstract ITaskInteraction TestGetInteraction(ITask task);
        }

        [Fact]
        public void InteractionIsDisposed()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            ITaskRegistry taskRegistry = Substitute.For<ITaskRegistry>();
            ITaskInteraction interaction = Substitute.For<ITaskInteraction, IDisposable>();

            TestTaskExecution execution = Substitute.ForPartsOf<TestTaskExecution>(arguments, taskRegistry, null);
            execution.TestGetInteraction(Arg.Any<ITask>()).Returns(interaction);
            execution.ExecuteTask();
            ((IDisposable)interaction).Received(1).Dispose();
        }
    }
}
