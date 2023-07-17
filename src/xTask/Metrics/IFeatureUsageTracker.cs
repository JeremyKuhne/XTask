// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Metrics
{
    public interface IFeatureUsageTracker
    {
        void RecordUsage(int featureIdentifier);
    }
}
