// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Build
{
    using Services;
    using XTask.Settings;
    using XTask.Tasks;
    using MSBuildFramework = Microsoft.Build.Framework;

    public class BuildTaskExecution : TaskExecution
    {
        private MSBuildFramework.IBuildEngine buildEngine;
        private ITaskOutputHandler outputHandler;

        /// <summary>
        /// Execution handler for tasks running under MSBuild.
        /// </summary>
        /// <param name="services">Override services, can be null. Used to get services before falling back on defaults.</param>
        public BuildTaskExecution(MSBuildFramework.IBuildEngine buildEngine, ITaskOutputHandler outputHandler, IArgumentProvider argumentProvider, ITaskRegistry taskRegistry, ITypedServiceProvider services = null)
            : base(argumentProvider, taskRegistry, services)
        {
            this.outputHandler = outputHandler;
            this.buildEngine = buildEngine;
        }

        protected override ITaskInteraction GetInteraction(ITask task)
        {
            return BuildTaskInteraction.Create(this.buildEngine, this.outputHandler, task, this.ArgumentProvider, this.Services);
        }
    }
}