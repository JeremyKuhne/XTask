// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;

    public class SimpleTaskRegistry : TaskRegistry
    {
        public void RegisterTask(Func<ITask> task, params string[] taskNames)
        {
            this.RegisterTaskInternal(task, taskNames);
        }

        public void RegisterDefaultTask(Func<ITask> task)
        {
            this.RegisterDefaultTaskInternal(task);
        }
    }
}
