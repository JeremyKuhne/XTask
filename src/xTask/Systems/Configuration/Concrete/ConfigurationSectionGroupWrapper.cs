// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Configuration;

namespace XTask.Systems.Configuration.Concrete
{
    public class ConfigurationSectionGroupWrapper : IConfigurationSectionGroup
    {
        private readonly ConfigurationSectionGroup _sectionGroup;

        public ConfigurationSectionGroupWrapper(ConfigurationSectionGroup sectionGroup) => _sectionGroup = sectionGroup;

        public ClientSettingsSection Get(string name) => _sectionGroup.Sections.Get(name) as ClientSettingsSection;

        public void Add(string name, ClientSettingsSection section) => _sectionGroup.Sections.Add(name, section);
    }
}
