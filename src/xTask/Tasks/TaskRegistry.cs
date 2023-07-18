// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace XTask.Tasks
{
    public abstract class TaskRegistry : ITaskRegistry
    {
        private class TaskEntry : ITaskEntry
        {
            private readonly HashSet<string> _aliases;
            private readonly Lazy<ITask> _task;

            public IEnumerable<string> Aliases => _aliases;

            public ITask Task => _task.Value;

            public TaskEntry(Func<ITask> task, params string[] taskNames)
            {
                _aliases = new HashSet<string>(taskNames, StringComparer.OrdinalIgnoreCase);
                _task = new Lazy<ITask>(task);
            }
        }

        private readonly List<TaskEntry> _tasks = new();
        private Func<ITask> _defaultTask;

        public IEnumerable<ITaskEntry> Tasks => _tasks;

        protected void RegisterTaskInternal(Func<ITask> task, params string[] taskNames)
            => _tasks.Add(new TaskEntry(task, taskNames));

        protected void RegisterDefaultTaskInternal(Func<ITask> task) => _defaultTask = task;

        public ITask this[string taskName]
        {
            get
            {
                if (!string.IsNullOrEmpty(taskName))
                {
                    foreach (var entry in _tasks)
                    {
                        if (entry.Aliases.Contains(taskName))
                        {
                            return entry.Task;
                        }
                    }
                }

                if (_defaultTask is not null)
                {
                    return _defaultTask();
                }
                else
                {
                    return new UnknownTask(this, XTaskStrings.HelpGeneral);
                }
            }
        }
    }
}