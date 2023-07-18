// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Logging;

namespace XTask.Tasks;

internal static class TaskExtensions
{
    /// <summary>
    ///  Gets the option default for the given option.
    /// </summary>
    internal static T GetOptionDefault<T>(this ITask task, string option)
    {
        ITaskOptionDefaults optionDefaults = task.GetService<ITaskOptionDefaults>();
        if (optionDefaults is not null)
        {
            return optionDefaults.GetOptionDefault<T>(option);
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    ///  Outputs usage if any help is provided.
    /// </summary>
    public static void OutputUsage(this ITask task, ITaskInteraction interaction)
    {
        ITaskDocumentation documentation = task.GetService<ITaskDocumentation>();
        if (documentation is null)
        {
            interaction.Loggers[LoggerType.Result].WriteLine(WriteStyle.Fixed, XTaskStrings.HelpNone);
        }
        else
        {
            documentation.GetUsage(interaction);
        }
    }

    /// <summary>
    ///  Executes the given task.
    /// </summary>
    public static ExitCode Execute(this ITask task, ITaskInteraction interaction)
    {
        ITaskExecutor executor = task.GetService<ITaskExecutor>();
        if (executor is not null)
        {
            return executor.Execute(interaction);
        }
        else
        {
            return ExitCode.Success;
        }
    }
}