// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System.Collections.Generic;

    public class Loggers : ILoggers
    {
        private Dictionary<LoggerType, ILogger> _loggers = new Dictionary<LoggerType, ILogger>();
        private static NullLogger NullLog = new NullLogger();

        protected void RegisterLogger(LoggerType loggerType, ILogger logger)
        {
            _loggers.Add(loggerType, logger);
        }

        public ILogger this[LoggerType loggerType]
        {
            get
            {
                ILogger logger;
                if (_loggers.TryGetValue(loggerType, out logger))
                {
                    return logger;
                }
                return NullLog;
            }
        }

        private class NullLogger : ILogger
        {
            public void Write(string value) { }
            public void Write(string format, params object[] args) { }
            public void Write(WriteStyle style, string format, params object[] args) { }
            public void Write(WriteStyle style, string value) { }
            public void WriteLine() { }
            public void WriteLine(string value) { }
            public void WriteLine(string format, params object[] args) { }
            public void WriteLine(WriteStyle style, string format, params object[] args) { }
            public void WriteLine(WriteStyle style, string value) { }
            public void Write(ITable table) { }
        }
    }
}