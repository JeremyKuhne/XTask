// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class TaskService : ITaskService
    {
        private string applicationName;
        private string generalHelpString;

        private Lazy<SimpleTaskRegistry> taskRegistry;

        protected TaskService(
            string generalHelpString = null,
            string applicationName = null)
        {
            this.taskRegistry = new Lazy<SimpleTaskRegistry>(() =>
            {
                return new SimpleTaskRegistry();
            });

            this.applicationName = applicationName ?? Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            this.generalHelpString = generalHelpString ?? XTaskStrings.HelpGeneral;
        }

        public virtual void Initialize()
        {
            SimpleTaskRegistry registry = this.GetTaskRegistry();

            // These commands are provided as part of the XTask framework
            registry.RegisterTask(() => new DefaultsTask(this.applicationName), "defaults");
            registry.RegisterTask(() => new InteractiveTask($"({this.applicationName}) ", registry), "interactive", "int", "i");
            registry.RegisterTask(() => new HelpTask(registry, this.generalHelpString), "help", "?");
            registry.RegisterDefaultTask(() => new UnknownTask(registry, this.generalHelpString));
        }

        protected virtual SimpleTaskRegistry GetTaskRegistry()
        {
            return this.taskRegistry.Value;
        }

        public ITaskRegistry TaskRegistry
        {
            get { return this.GetTaskRegistry(); }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        public virtual void Dispose(bool disposing)
        {
        }
    }
}
