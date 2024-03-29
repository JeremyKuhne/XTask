﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetConfiguration = System.Configuration;

namespace XTask.Systems.Configuration.Concrete;

/// <summary>
///  Simple wrapper to abstract the .NET ConfigurationManager.
/// </summary>
public class ConfigurationManager : IConfigurationManager
{
    public IConfiguration OpenConfiguration(string filePath)
    {
        DotNetConfiguration.Configuration configuration = DotNetConfiguration.ConfigurationManager.OpenMappedExeConfiguration(
            new DotNetConfiguration.ExeConfigurationFileMap() { ExeConfigFilename = filePath },
            DotNetConfiguration.ConfigurationUserLevel.None);

        return configuration is null ? null : new ConfigurationWrapper(configuration);
    }

    public IConfiguration OpenConfiguration(DotNetConfiguration.ConfigurationUserLevel userLevel)
    {
        DotNetConfiguration.Configuration configuration = DotNetConfiguration.ConfigurationManager.OpenExeConfiguration(userLevel);
        return configuration is null ? null : new ConfigurationWrapper(configuration);
    }
}
