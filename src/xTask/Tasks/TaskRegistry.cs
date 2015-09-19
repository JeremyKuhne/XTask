// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class TaskRegistry : ITaskRegistry
    {
        private class TaskEntry : ITaskEntry
        {
            private HashSet<string> aliases;
            private Lazy<ITask> task;

            public IEnumerable<string> Aliases { get { return this.aliases; } }

            public ITask Task { get { return task.Value; } }

            public TaskEntry(Func<ITask> task, params string[] taskNames)
            {
                this.aliases = new HashSet<string>(taskNames, StringComparer.OrdinalIgnoreCase);
                this.task = new Lazy<ITask>(task);
            }
        }

        private List<TaskEntry> tasks = new List<TaskEntry>();
        private Func<ITask> defaultTask;

        public IEnumerable<ITaskEntry> Tasks { get { return this.tasks; } }

        protected void RegisterTaskInternal(Func<ITask> task, params string[] taskNames)
        {
            this.tasks.Add(new TaskEntry(task, taskNames));
        }

        protected void RegisterDefaultTaskInternal(Func<ITask> task)
        {
            this.defaultTask = task;
        }

        public ITask this[string taskName]
        {
            get
            {
                if (!String.IsNullOrEmpty(taskName))
                {
                    foreach (var entry in this.tasks)
                    {
                        if (entry.Aliases.Contains(taskName))
                        {
                            return entry.Task;
                        }
                    }
                }

                if (this.defaultTask != null)
                {
                    return this.defaultTask();
                }
                else
                {
                    return new UnknownTask(this, XTaskStrings.HelpGeneral);
                }
            }
        }
    }
}