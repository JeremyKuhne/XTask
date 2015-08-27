// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;
    using XTask.Logging;
    using XTask.Services;
    using XTask.Utility;

    /// <summary>
    /// General help task
    /// </summary>
    public class HelpTask : ImplementedServiceProvider, ITask, ITaskDocumentation, ITaskExecutor
    {
        private bool unknownCommand;
        private string generalHelp;

        public HelpTask(bool unknownCommand, string generalHelp)
        {
            this.unknownCommand = unknownCommand;
            this.generalHelp = generalHelp;
        }

        public ExitCode Execute(ITaskInteraction interaction)
        {
            if (this.unknownCommand)
            {
                if (String.IsNullOrEmpty(interaction.Arguments.Command))
                {
                    interaction.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XTaskStrings.ErrorNoParametersSpecified);
                }
                else
                {
                    interaction.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XTaskStrings.UnknownCommand, interaction.Arguments.Command);
                }
            }

            interaction.Loggers[LoggerType.Result].WriteLine(WriteStyle.Fixed, this.generalHelp);
            return this.unknownCommand ? ExitCode.InvalidArgument : ExitCode.Success;
        }

        public void GetUsage(ITaskInteraction interaction)
        {
            this.Execute(interaction);
        }
    }
}