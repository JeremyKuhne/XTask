// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Services;
using XTask.Settings;

namespace XTask.Tasks;

public abstract class TaskExecution
{
    private readonly ITaskRegistry _taskRegistry;

    protected TaskExecution(IArgumentProvider argumentProvider, ITaskRegistry taskRegistry, ITypedServiceProvider services)
    {
        ArgumentProvider = argumentProvider;
        _taskRegistry = taskRegistry;
        Services = services;
    }

    protected IArgumentProvider ArgumentProvider { get; private set; }
    protected abstract ITaskInteraction GetInteraction(ITask task);
    protected ITypedServiceProvider Services { get; private set; }

    public ExitCode ExecuteTask()
    {
        ITask task = _taskRegistry[ArgumentProvider.Command];
        ITaskInteraction interaction = GetInteraction(task);

        ExitCode result = ExitCode.GeneralFailure;

        using (interaction as IDisposable)
        using (task as IDisposable)
        {
            if (ArgumentProvider.HelpRequested)
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