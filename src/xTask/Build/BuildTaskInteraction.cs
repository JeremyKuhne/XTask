﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Logging;
using XTask.Services;
using XTask.Settings;
using XTask.Tasks;
using MSBuildFramework = Microsoft.Build.Framework;

namespace XTask.Build;

public sealed class BuildTaskInteraction : TaskInteraction
{
    private readonly ITaskOutputHandler _outputHandler;
    private readonly Lazy<BuildTaskLoggers> _loggers;

    private BuildTaskInteraction(
        ITask task,
        IArgumentProvider arguments,
        ITaskOutputHandler outputHandler,
        MSBuildFramework.IBuildEngine buildEngine,
        ITypedServiceProvider services)
        : base (arguments, services)
    {
        _outputHandler = outputHandler;
        _loggers = new Lazy<BuildTaskLoggers>(() => new BuildTaskLoggers(buildEngine, task, arguments));
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

    public override void Output(object value) => _outputHandler.HandleOutput(value);

    protected override ILoggers GetDefaultLoggers() => _loggers.Value;

    private sealed class BuildTaskLoggers : Loggers
    {
        public BuildTaskLoggers(MSBuildFramework.IBuildEngine buildEngine, ITask task, IArgumentProvider arguments)
        {
            BuildLogger logger = new(buildEngine, task.GetType().ToString());
            RegisterLogger(LoggerType.Result, logger);
            RegisterLogger(LoggerType.Status, logger);
        }
    }
}