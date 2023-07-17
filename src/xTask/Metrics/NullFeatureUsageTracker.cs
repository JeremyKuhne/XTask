// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Metrics
{
    /// <summary>
    ///  Stub feature usage tracker.
    /// </summary>
    public class NullFeatureUsageTracker : IFeatureUsageTracker
    {
        public void RecordUsage(int featureIdentifier)
        {
            // Do nothing
        }
    }
}
