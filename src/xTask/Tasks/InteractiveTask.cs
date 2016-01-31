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
    using Systems.File;
    using Systems.Configuration;
    using Utility;

    /// <summary>
    /// Puts task into an interactive mode where multiple commands can be entered.
    /// </summary>
    public class InteractiveTask : Task
    {
        private string _prompt;
        private IConsoleService _consoleService;
        private ITaskRegistry _registry;
        private static readonly string[] s_QuitCommands = { "quit", "q", "exit" };

        public InteractiveTask(string prompt, ITaskRegistry registry, IConsoleService consoleService = null)
        {
            _prompt = prompt;
            _registry = registry;
            _consoleService = consoleService ?? ConsoleHelper.Console;
        }

        protected override ExitCode ExecuteInternal()
        {
            string input = null;
            do
            {
                _consoleService.Write(_prompt);
                input = Environment.ExpandEnvironmentVariables(_consoleService.ReadLine().Trim());
                if (s_QuitCommands.Contains(input, StringComparer.OrdinalIgnoreCase)) break;
                CommandLineParser parser = new CommandLineParser(GetService<IFileService>());
                parser.Parse(Strings.SplitCommandLine(input).ToArray());
                IArgumentProvider argumentProvider = ArgumentSettingsProvider.Create(parser, GetService<IConfigurationManager>(), GetService<IFileService>());
                ConsoleTaskExecution execution = new ConsoleTaskExecution(argumentProvider, _registry);
                execution.ExecuteTask();
            } while (true);

            return ExitCode.Success;
        }

        public override string Summary { get { return XTaskStrings.InteractiveTaskSummary; } }
    }
}
