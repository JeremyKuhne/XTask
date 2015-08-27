// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;
    using Services;
    using XTask.Settings;
    using XTask.Utility;

    public abstract class TaskExecution
    {
        private ITaskRegistry taskRegistry;
        protected TaskExecution(IArgumentProvider argumentProvider, ITaskRegistry taskRegistry, ITypedServiceProvider services)
        {
            this.ArgumentProvider = argumentProvider;
            this.taskRegistry = taskRegistry;
            this.Services = services;
        }

        protected IArgumentProvider ArgumentProvider { get; private set; }
        protected abstract ITaskInteraction GetInteraction(ITask task);
        protected ITypedServiceProvider Services { get; private set; }

        public ExitCode ExecuteTask()
        {
            ITask task = this.taskRegistry[this.ArgumentProvider.Command];
            ITaskInteraction interaction = this.GetInteraction(task);

            ExitCode result = ExitCode.GeneralFailure;

            using (interaction as IDisposable)
            using (task as IDisposable)
            {
                if (this.ArgumentProvider.HelpRequested)
                {
                    task.OutputUsage(interaction);
                }
                else
                {
                    result = task.Execute(interaction);
                }
            }

            return result;
        }
    }
}