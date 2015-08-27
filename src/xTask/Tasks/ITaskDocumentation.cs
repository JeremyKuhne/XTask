// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    // In theory it would be nice to make help details attributes, but they only allow constant values.
    // We want string table strings- standard resource generation generation does not make constants for lookup.
    // While one could use the resource name and look it up you would end up only finding missing/mismatched
    // resources at runtime. Prefer a compile time failure instead.

    // Also considered keeping the task help in a central location, but want to lean towards keeping
    // metadata associated with the task in/on the task class itself for ease of maintenance.

    /// <summary>
    /// Standard task help interface
    /// </summary>
    public interface ITaskDocumentation
    {
        void GetUsage(ITaskInteraction interaction);
    }
}