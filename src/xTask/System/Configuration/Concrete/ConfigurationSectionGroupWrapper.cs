// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.ConfigurationSystem.Concrete
{
    using System.Configuration;

    public class ConfigurationSectionGroupWrapper : IConfigurationSectionGroup
    {
        ConfigurationSectionGroup sectionGroup;

        public ConfigurationSectionGroupWrapper(ConfigurationSectionGroup sectionGroup)
        {
            this.sectionGroup = sectionGroup;
        }

        public ClientSettingsSection Get(string name)
        {
            return this.sectionGroup.Sections.Get(name) as ClientSettingsSection;
        }

        public void Add(string name, ClientSettingsSection section)
        {
            this.sectionGroup.Sections.Add(name, section);
        }
    }
}
