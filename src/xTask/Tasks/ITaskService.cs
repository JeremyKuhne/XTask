// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using System;

    public interface ITaskService : IDisposable
    {
        ITaskRegistry TaskRegistry { get; }
    }
}
