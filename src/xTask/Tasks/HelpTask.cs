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
        private string generalHelp;

        public HelpTask(string generalHelp)
        {
            this.generalHelp = generalHelp;
        }

        public virtual ExitCode Execute(ITaskInteraction interaction)
        {
            interaction.Loggers[LoggerType.Result].WriteLine(WriteStyle.Fixed, this.generalHelp);
            return ExitCode.Success;
        }

        public void GetUsage(ITaskInteraction interaction)
        {
            this.Execute(interaction);
        }
    }
}