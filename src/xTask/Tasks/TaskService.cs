// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace XTask.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly string _applicationName;
        private readonly string _generalHelpString;

        private readonly Lazy<SimpleTaskRegistry> _taskRegistry;

        protected TaskService(
            string generalHelpString = null,
            string applicationName = null)
        {
            _taskRegistry = new Lazy<SimpleTaskRegistry>(() =>
            {
                return new SimpleTaskRegistry();
            });

            _applicationName = applicationName ?? Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
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

        protected virtual SimpleTaskRegistry GetTaskRegistry() => _taskRegistry.Value;

        public ITaskRegistry TaskRegistry => GetTaskRegistry();

        public void Dispose() => Dispose(disposing: true);

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
