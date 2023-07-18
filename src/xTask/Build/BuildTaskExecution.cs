// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Services;
using XTask.Settings;
using XTask.Tasks;
using MSBuildFramework = Microsoft.Build.Framework;

namespace XTask.Build
{
    public class BuildTaskExecution : TaskExecution
    {
        private readonly MSBuildFramework.IBuildEngine _buildEngine;
        private readonly ITaskOutputHandler _outputHandler;

        /// <summary>
        ///  Execution handler for tasks running under MSBuild.
        /// </summary>
        /// <param name="services">
        ///  Override services, can be <see langword="null"/>. Used to get services before falling back on defaults.
        /// </param>
        public BuildTaskExecution(
            MSBuildFramework.IBuildEngine buildEngine,
            ITaskOutputHandler outputHandler,
            IArgumentProvider argumentProvider,
            ITaskRegistry taskRegistry,
            ITypedServiceProvider services = null)
            : base(argumentProvider, taskRegistry, services)
        {
            _outputHandler = outputHandler;
            _buildEngine = buildEngine;
        }

        protected override ITaskInteraction GetInteraction(ITask task)
            => BuildTaskInteraction.Create(_buildEngine, _outputHandler, task, ArgumentProvider, Services);
    }
}