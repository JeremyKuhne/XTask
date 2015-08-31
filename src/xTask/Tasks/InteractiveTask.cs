// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using Settings;
    using System;
    using System.Linq;
    using Systems.Console;
    using XTask.Utility;

    /// <summary>
    /// Puts task into an interactive mode where multiple commands can be entered.
    /// </summary>
    public class InteractiveTask : Tasks.Task
    {
        private string prompt;
        private IConsoleService consoleService;
        private ITaskRegistry registry;
        private static readonly string[] quitCommands = { "quit", "q", "exit" };

        public InteractiveTask(string prompt, ITaskRegistry registry, IConsoleService consoleService = null)
        {
            this.prompt = prompt;
            this.registry = registry;
            this.consoleService = consoleService ?? ConsoleHelper.Console;
        }

        protected override ExitCode ExecuteInternal()
        {
            string input = null;
            do
            {
                this.consoleService.Write(this.prompt);
                input = Environment.ExpandEnvironmentVariables(this.consoleService.ReadLine().Trim());
                if (InteractiveTask.quitCommands.Contains(input, StringComparer.OrdinalIgnoreCase)) break;
                CommandLineParser parser = new CommandLineParser();
                parser.Parse(Strings.SplitCommandLine(input).ToArray());
                IArgumentProvider argumentProvider = ArgumentSettingsProvider.Create(parser);
                ConsoleTaskExecution execution = new ConsoleTaskExecution(argumentProvider, this.registry);
                execution.ExecuteTask();
            } while (true);

            return ExitCode.Success;
        }
    }
}
