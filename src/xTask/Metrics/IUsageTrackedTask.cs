// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Metrics
{
    using XTask.Tasks;

    public interface IUsageTrackedTask : ITask
    {
        int TaskFeatureIdentifier { get; }
    }
}
