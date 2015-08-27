// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Build
{
    using System;
    using XTask.Logging;
    using Services;
    using XTask.Settings;
    using XTask.Tasks;
    using MSBuildFramework = Microsoft.Build.Framework;

    public sealed class BuildTaskInteraction : TaskInteraction
    {
        private ITaskOutputHandler outputHandler;
        private Lazy<BuildTaskLoggers> loggers;

        private BuildTaskInteraction(
            ITask task,
            IArgumentProvider arguments,
            ITaskOutputHandler outputHandler,
            MSBuildFramework.IBuildEngine buildEngine,
            ITypedServiceProvider services)
            : base (arguments, services)
        {
            this.outputHandler = outputHandler;
            this.loggers = new Lazy<BuildTaskLoggers>(() => new BuildTaskLoggers(buildEngine, task, arguments));
        }

        public static ITaskInteraction Create(
            MSBuildFramework.IBuildEngine buildEngine,
            ITaskOutputHandler outputHandler,
            ITask task,
            IArgumentProvider arguments,
            ITypedServiceProvider services)
        {
            return new BuildTaskInteraction(task, arguments, outputHandler, buildEngine, services);
        }

        public override void Output(object value)
        {
            this.outputHandler.HandleOutput(value);
        }

        protected override ILoggers GetDefaultLoggers()
        {
            return this.loggers.Value;
        }

        private sealed class BuildTaskLoggers : Loggers
        {
            public BuildTaskLoggers(MSBuildFramework.IBuildEngine buildEngine, ITask task, IArgumentProvider arguments)
            {
                BuildLogger logger = new BuildLogger(buildEngine, task.GetType().ToString());
                this.RegisterLogger(LoggerType.Result, logger);
                this.RegisterLogger(LoggerType.Status, logger);
            }
        }
    }
}