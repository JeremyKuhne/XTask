// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Tasks;

public class SimpleTaskRegistry : TaskRegistry
{
    public void RegisterTask(Func<ITask> task, params string[] taskNames) => RegisterTaskInternal(task, taskNames);

    public void RegisterDefaultTask(Func<ITask> task) => RegisterDefaultTaskInternal(task);
}
