// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Tasks
{
    using FluentAssertions;
    using NSubstitute;
    using XTask.Tasks;
    using Xunit;

    public class TaskServiceTests
    {
        [Theory,
            InlineData("defaults"),
            InlineData("interactive"),
            InlineData("help")]
        public void HasExpectedTasks(string service)
        {
            TaskService taskService = Substitute.ForPartsOf<TaskService>("Help", "MyApp");
            taskService.Initialize();
            taskService.TaskRegistry[service].Should().NotBeNull();
        }
    }
}
