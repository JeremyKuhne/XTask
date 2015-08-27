// ----------------------
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
        private IFeatureUsageTracker usageTracker;

        public TrackedTaskRegistry(IFeatureUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
        }

        public void RegisterTask(Func<ITask> task, int featureIdentifier, params string[] taskNames)
        {
            this.RegisterTaskInternal(() => new TrackedTask(task(), featureIdentifier, usageTracker), taskNames);
        }

        public void RegisterDefaultTask(Func<ITask> task, int featureIdentifier)
        {
            this.RegisterDefaultTaskInternal(() => new TrackedTask(task(), featureIdentifier, usageTracker));
        }
    }
}
