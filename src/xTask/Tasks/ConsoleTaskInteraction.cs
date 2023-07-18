// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Services;
using XTask.Logging;
using XTask.Settings;

namespace XTask.Tasks;

public sealed class ConsoleTaskInteraction : TaskInteraction, IDisposable
{
    private Lazy<ConsoleTaskLoggers> _loggers;

    private ConsoleTaskInteraction(ITask task, IArgumentProvider arguments, ITypedServiceProvider services)
        : base (arguments, services)
    {
        _loggers = new Lazy<ConsoleTaskLoggers>(() => new ConsoleTaskLoggers(task, arguments));
    }

    public static ITaskInteraction Create(ITask task, IArgumentProvider arguments, ITypedServiceProvider services)
        => new ConsoleTaskInteraction(task, arguments, services);

    protected override ILoggers GetDefaultLoggers() => _loggers.Value;

    private sealed class ConsoleTaskLoggers : Loggers, IDisposable
    {
        private RichTextLogger _richTextLogger;
        private readonly TextLogger _textLogger;
        private readonly CsvLogger _csvLogger;
        private readonly XmlSpreadsheetLogger _spreadsheetLogger;
        private readonly AggregatedLogger _aggregatedLogger;

        public ConsoleTaskLoggers(ITask task, IArgumentProvider arguments)
        {
            if (arguments.GetOption<bool?>(StandardOptions.Clipboard) ?? task.GetOptionDefault<bool>(StandardOptions.Clipboard[0]))
            {
                _richTextLogger = new RichTextLogger();
                _csvLogger = new CsvLogger();
                _textLogger = new TextLogger();
                _spreadsheetLogger = new XmlSpreadsheetLogger();
                _aggregatedLogger = new AggregatedLogger(
                    ConsoleLogger.Instance,
                    _richTextLogger,
                    _spreadsheetLogger,
                    _csvLogger,
                    _textLogger);

                RegisterLogger(LoggerType.Result, _aggregatedLogger);
            }
            else
            {
                RegisterLogger(LoggerType.Result, ConsoleLogger.Instance);
            }

            RegisterLogger(LoggerType.Status, ConsoleLogger.Instance);
        }

        public void Dispose()
        {
            if (_aggregatedLogger is not null)
            {
                List<ClipboardData> allData = new()
                {
                    _richTextLogger.GetClipboardData(),
                    _textLogger.GetClipboardData(),
                    _csvLogger.GetClipboardData(),
                    _spreadsheetLogger.GetClipboardData()
                };

                ClipboardHelper.SetClipboardData(allData.ToArray());
                _richTextLogger = null;

                _csvLogger.Dispose();
                _spreadsheetLogger.Dispose();
            }
        }
    }

    public void Dispose()
    {
        if (_loggers.IsValueCreated) _loggers.Value.Dispose();
        _loggers = null;
    }
}