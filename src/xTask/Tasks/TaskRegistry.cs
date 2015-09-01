// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;
    using System.Collections.Generic;

    public abstract class TaskRegistry : ITaskRegistry
    {
        private Dictionary<string, Func<ITask>> tasks = new Dictionary<string, Func<ITask>>(StringComparer.OrdinalIgnoreCase);

        private Func<ITask> defaultTask;

        protected void RegisterTaskInternal(Func<ITask> task, params string[] taskNames)
        {
            foreach (string taskName in taskNames)
            {
                this.tasks.Add(taskName, task);
            }
        }

        protected void RegisterDefaultTaskInternal(Func<ITask> task)
        {
            this.defaultTask = task;
        }

        public ITask this[string taskName]
        {
            get
            {
                Func<ITask> task = null;
                if (String.IsNullOrEmpty(taskName) || !this.tasks.TryGetValue(taskName, out task))
                {
                    if (this.defaultTask != null)
                    {
                        return this.defaultTask();
                    }
                    else
                    {
                        return new UnknownTask(XTaskStrings.HelpGeneral);
                    }
                }
                return task();
            }
        }
    }
}