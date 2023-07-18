// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    /// <summary>
    ///  Interface for execution of a task.
    /// </summary>
    public interface ITaskExecutor
    {
        /// <summary>
        ///  Execute the given task.
        /// </summary>
        ExitCode Execute(ITaskInteraction interaction);
    }
}
