﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Tasks
{
    public interface ITaskService : IDisposable
    {
        ITaskRegistry TaskRegistry { get; }
    }
}
