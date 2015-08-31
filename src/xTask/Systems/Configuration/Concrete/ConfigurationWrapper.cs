// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.Configuration.Concrete
{
    using System.Configuration;

    /// <summary>
    /// Simple wrapper to abstract a configuration object
    /// </summary>
    public class ConfigurationWrapper : IConfiguration
    {
        private Configuration configuration;

        public ConfigurationWrapper(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public IConfigurationSectionGroup GetSectionGroup(string sectionGroupName)
        {
            ConfigurationSectionGroup sectionGroup = this.configuration.GetSectionGroup(sectionGroupName);
            return sectionGroup == null ? null : new ConfigurationSectionGroupWrapper(sectionGroup);
        }

        public string FilePath
        {
            get { return this.configuration.FilePath; }
        }

        public IConfigurationSectionGroup AddSectionGroup(string sectionGroupName)
        {
            UserSettingsGroup userGroup = new UserSettingsGroup();
            this.configuration.SectionGroups.Add(sectionGroupName, userGroup);

            return new ConfigurationSectionGroupWrapper(userGroup);
        }

        public void Save()
        {
            this.configuration.Save();
        }
    }
}
