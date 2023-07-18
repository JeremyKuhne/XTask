// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Services;
using XTask.Logging;
using XTask.Settings;

namespace XTask.Tasks;

/// <summary>
///  Input and output interfaces and services for a task.
/// </summary>
public interface ITaskInteraction : ITypedServiceProvider
{
    IArgumentProvider Arguments { get; }
    ILoggers Loggers { get; }
    void Output(object value);
}
