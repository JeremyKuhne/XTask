// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using Services;
    using XTask.Logging;
    using XTask.Settings;

    /// <summary>
    /// Input and output interfaces and services for a task
    /// </summary>
    public interface ITaskInteraction : ITypedServiceProvider
    {
        IArgumentProvider Arguments { get; }
        ILoggers Loggers { get; }
        void Output(object value);
    }
}
