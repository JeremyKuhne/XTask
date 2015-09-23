// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using Services;
    using System;
    using System.Collections.Generic;
    using XTask.Logging;
    using XTask.Settings;

    public sealed class ConsoleTaskInteraction : TaskInteraction, IDisposable
    {
        private Lazy<ConsoleTaskLoggers> loggers;

        private ConsoleTaskInteraction(ITask task, IArgumentProvider arguments, ITypedServiceProvider services)
            : base (arguments, services)
        {
            this.loggers = new Lazy<ConsoleTaskLoggers>(() => new ConsoleTaskLoggers(task, arguments));
        }

        public static ITaskInteraction Create(ITask task, IArgumentProvider arguments, ITypedServiceProvider services)
        {
            return new ConsoleTaskInteraction(task, arguments, services);
        }

        protected override ILoggers GetDefaultLoggers()
        {
            return this.loggers.Value;
        }

        private sealed class ConsoleTaskLoggers : Loggers, IDisposable
        {
            private RichTextLogger richTextLogger;
            private TextLogger textLogger;
            private CsvLogger csvLogger;
            private XmlSpreadsheetLogger spreadsheetLogger;
            private AggregatedLogger aggregatedLogger;

            public ConsoleTaskLoggers(ITask task, IArgumentProvider arguments)
            {
                if (arguments.GetOption<bool?>(StandardOptions.Clipboard) ?? task.GetOptionDefault<bool>(StandardOptions.Clipboard[0]))
                {
                    this.richTextLogger = new RichTextLogger();
                    this.csvLogger = new CsvLogger();
                    this.textLogger = new TextLogger();
                    this.spreadsheetLogger = new XmlSpreadsheetLogger();
                    this.aggregatedLogger = new AggregatedLogger(
                        ConsoleLogger.Instance,
                        this.richTextLogger,
                        this.spreadsheetLogger,
                        this.csvLogger,
                        this.textLogger);

                    this.RegisterLogger(LoggerType.Result, this.aggregatedLogger);
                }
                else
                {
                    this.RegisterLogger(LoggerType.Result, ConsoleLogger.Instance);
                }

                this.RegisterLogger(LoggerType.Status, ConsoleLogger.Instance);
            }

            public void Dispose()
            {
                if (this.aggregatedLogger != null)
                {
                    List<ClipboardData> allData = new List<ClipboardData>();
                    allData.Add(this.richTextLogger.GetClipboardData());
                    allData.Add(this.textLogger.GetClipboardData());
                    allData.Add(this.csvLogger.GetClipboardData());
                    allData.Add(this.spreadsheetLogger.GetClipboardData());

                    Clipboard.AddToClipboard(allData.ToArray());
                    this.richTextLogger = null;

                    this.csvLogger.Dispose();
                    this.spreadsheetLogger.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (this.loggers.IsValueCreated) this.loggers.Value.Dispose();
            this.loggers = null;
        }
    }
}