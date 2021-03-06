﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Metrics
{
    using System;
    using XTask.Tasks;

    public class TrackedTaskRegistry : TaskRegistry
    {
        private IFeatureUsageTracker _usageTracker;

        public TrackedTaskRegistry(IFeatureUsageTracker usageTracker)
        {
            _usageTracker = usageTracker;
        }

        public void RegisterTask(Func<ITask> task, int featureIdentifier, params string[] taskNames)
        {
            RegisterTaskInternal(() => new TrackedTask(task(), featureIdentifier, _usageTracker), taskNames);
        }

        public void RegisterDefaultTask(Func<ITask> task, int featureIdentifier)
        {
            RegisterDefaultTaskInternal(() => new TrackedTask(task(), featureIdentifier, _usageTracker));
        }
    }
}
