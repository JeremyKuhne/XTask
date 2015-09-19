// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;
    using XTask.Logging;
    using XTask.Utility;

    /// <summary>
    /// Help for unknown commands
    /// </summary>
    public class UnknownTask : HelpTask
    {
        public UnknownTask(ITaskRegistry registry, string generalHelp)
            : base (registry, generalHelp)
        {
        }

        public override ExitCode Execute(ITaskInteraction interaction)
        {
            if (String.IsNullOrEmpty(interaction.Arguments.Command))
            {
                interaction.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XTaskStrings.ErrorNoParametersSpecified);
            }
            else
            {
                interaction.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XTaskStrings.UnknownCommand, interaction.Arguments.Command);
            }

            base.Execute(interaction);
            return ExitCode.InvalidArgument;
        }
    }
}