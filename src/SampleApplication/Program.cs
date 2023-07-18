// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Services;
using XTask.Settings;
using XTask.Systems.Configuration;

namespace XFile;

internal class Program
{
    [STAThread] // Need to be STA to use OLE (clipboard) and WPF
    private static int Main(string[] args)
    {
        ExitCode result = ExitCode.GeneralFailure;

        CommandLineParser parser = new(FlexServiceProvider.Services.GetService<IFileService>());
        parser.Parse(args);

        IArgumentProvider argumentProvider = ArgumentSettingsProvider.Create(parser, FlexServiceProvider.Services.GetService<IConfigurationManager>(), FlexServiceProvider.Services.GetService<IFileService>());

        using (ITaskService taskService = XFileTaskService.Create())
        {
            ConsoleTaskExecution execution = new(argumentProvider, taskService.TaskRegistry);
            result = execution.ExecuteTask();
        }

        return (int)result;
    }
}
