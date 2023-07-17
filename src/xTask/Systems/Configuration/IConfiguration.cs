// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.Configuration
{
    public interface IConfiguration
    {
        IConfigurationSectionGroup GetSectionGroup(string sectionGroupName);
        IConfigurationSectionGroup AddSectionGroup(string sectionGroupName);
        string FilePath { get; }
        void Save();
    }
}
