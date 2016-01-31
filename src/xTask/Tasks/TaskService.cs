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
        private string _applicationName;
        private string _generalHelpString;

        private Lazy<SimpleTaskRegistry> _taskRegistry;

        protected TaskService(
            string generalHelpString = null,
            string applicationName = null)
        {
            _taskRegistry = new Lazy<SimpleTaskRegistry>(() =>
            {
                return new SimpleTaskRegistry();
            });

            _applicationName = applicationName ?? Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            _generalHelpString = generalHelpString ?? XTaskStrings.HelpGeneral;
        }

        public virtual void Initialize()
        {
            SimpleTaskRegistry registry = GetTaskRegistry();

            // These commands are provided as part of the XTask framework
            registry.RegisterTask(() => new DefaultsTask(_applicationName), "defaults");
            registry.RegisterTask(() => new InteractiveTask($"({_applicationName}) ", registry), "interactive", "int", "i");
            registry.RegisterTask(() => new HelpTask(registry, _generalHelpString), "help", "?");
            registry.RegisterDefaultTask(() => new UnknownTask(registry, _generalHelpString));
        }

        protected virtual SimpleTaskRegistry GetTaskRegistry()
        {
            return _taskRegistry.Value;
        }

        public ITaskRegistry TaskRegistry
        {
            get { return GetTaskRegistry(); }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
