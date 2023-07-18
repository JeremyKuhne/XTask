// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using XTask.Tasks;

namespace XTask.Tests.Tasks;

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
