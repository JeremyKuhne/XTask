// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks;

/// <summary>
///  Task registry.
/// </summary>
public interface ITaskRegistry
{
    /// <summary>
    ///  Return the task with the given task name or alias, if any.
    /// </summary>
    ITask this[string taskName] { get; }

    IEnumerable<ITaskEntry> Tasks { get; }
}