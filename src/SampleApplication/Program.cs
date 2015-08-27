// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile
{
    using System;
    using XTask.Settings;
    using XTask.Tasks;
    using XTask.Utility;

    class Program
    {
        [STAThread] // Need to be STA to use OLE (clipboard) and WPF
        static int Main(string[] args)
        {
            ExitCode result = ExitCode.GeneralFailure;

            CommandLineParser parser = new CommandLineParser();
            parser.Parse(args);

            IArgumentProvider argumentProvider = ArgumentSettingsProvider.Create(parser);

            using (ITaskService taskService = XFileTaskService.Create())
            {
                ConsoleTaskExecution execution = new ConsoleTaskExecution(argumentProvider, taskService.TaskRegistry);
                result = execution.ExecuteTask();
            }

            return (int)result;
        }
    }
}
