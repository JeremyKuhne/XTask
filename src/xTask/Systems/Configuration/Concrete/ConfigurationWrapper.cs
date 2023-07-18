// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.Configuration.Concrete;

using System.Configuration;

/// <summary>
///  Simple wrapper to abstract a configuration object
/// </summary>
public class ConfigurationWrapper : IConfiguration
{
    private readonly Configuration _configuration;

    public ConfigurationWrapper(Configuration configuration)
    {
        _configuration = configuration;
    }

    public IConfigurationSectionGroup GetSectionGroup(string sectionGroupName)
    {
        ConfigurationSectionGroup sectionGroup = _configuration.GetSectionGroup(sectionGroupName);
        return sectionGroup is null ? null : new ConfigurationSectionGroupWrapper(sectionGroup);
    }

    public string FilePath => _configuration.FilePath;

    public IConfigurationSectionGroup AddSectionGroup(string sectionGroupName)
    {
        UserSettingsGroup userGroup = new();
        _configuration.SectionGroups.Add(sectionGroupName, userGroup);

        return new ConfigurationSectionGroupWrapper(userGroup);
    }

    public void Save() => _configuration.Save();
}
