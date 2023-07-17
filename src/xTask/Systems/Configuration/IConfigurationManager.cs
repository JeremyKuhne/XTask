// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Configuration;

namespace XTask.Systems.Configuration
{
    /// <summary>
    ///  Configuration management
    /// </summary>
    /// <remarks>
    ///  Aligns with the surface area of the .NET Configuration Manager
    /// </remarks>
    public interface IConfigurationManager
    {
        IConfiguration OpenConfiguration(string filePath);
        IConfiguration OpenConfiguration(ConfigurationUserLevel userLevel);
    }
}
