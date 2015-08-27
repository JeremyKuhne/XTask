// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.ConfigurationSystem.Concrete
{
    using System.Configuration;

    /// <summary>
    /// Simple wrapper to abstract the ConfigurationManager
    /// </summary>
    public class ConfigurationManagerWrapper : ConfigurationSystem.IConfigurationManager
    {
        public IConfiguration OpenConfiguration(string filePath)
        {
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap() { ExeConfigFilename = filePath },
                ConfigurationUserLevel.None);

            return configuration == null ? null : new ConfigurationWrapper(configuration);
        }

        public IConfiguration OpenConfiguration(ConfigurationUserLevel userLevel)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(userLevel);
            return configuration == null ? null : new ConfigurationWrapper(configuration);
        }
    }
}
