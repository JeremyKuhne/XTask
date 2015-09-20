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
    using XTask.Settings;
    using XTask.Utility;

    public abstract class Task : ImplementedServiceProvider, ITask, ITaskExecutor, ITaskDocumentation
    {
        private ITaskInteraction interaction;

        public ExitCode Execute(ITaskInteraction interaction)
        {
            this.interaction = interaction;
            return this.ExecuteInternal();
        }

        protected abstract ExitCode ExecuteInternal();

        protected IArgumentProvider Arguments { get { return this.interaction.Arguments; } }
        protected ILoggers Loggers { get { return this.interaction.Loggers; } }

        protected ILogger StatusLog { get { return this.Loggers[LoggerType.Status]; } }
        protected ILogger ResultLog { get { return this.Loggers[LoggerType.Result]; } }

        protected void Output(object value)
        {
            this.interaction.Output(value);
        }

        public override T GetService<T>()
        {
            // Try to get services from:
            // 1. Base (implementation)
            // 2. TaskInteraction
            // 3. DefaultServices

            return base.GetService<T>() ?? this.interaction?.GetService<T>() ?? DefaultServiceProvider.Services.GetService<T>();
        }

        public void GetUsage(ITaskInteraction interaction)
        {
            // Guidance for argument syntax:
            // IEEE Std 1003.1 http://pubs.opengroup.org/onlinepubs/009695399/basedefs/xbd_chap12.html

            ILogger logger = interaction.Loggers[LoggerType.Result];
            if (String.IsNullOrEmpty(this.GeneralHelp))
            {
                logger.Write(XTaskStrings.HelpNone);
                return;
            }

            logger.WriteLine(WriteStyle.Fixed, this.GeneralHelp);

            string optionDetails = this.OptionDetails;
            if (String.IsNullOrEmpty(optionDetails)) { return; }

            logger.WriteLine();
            logger.WriteLine(WriteStyle.Fixed | WriteStyle.Underline, XTaskStrings.HelpOptionDetailHeader);
            logger.WriteLine();
            logger.WriteLine(WriteStyle.Fixed, optionDetails);
        }


        protected virtual string GeneralHelp { get { return null; } }
        protected virtual string OptionDetails { get { return null; } }

        public virtual string Summary { get { return null; } }
    }
}