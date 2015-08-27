// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System.Collections.Generic;

    /// <summary>
    /// Access to client settings
    /// </summary>
    public interface IClientSettings
    {
        /// <summary>
        /// Save the given setting if possible, return false otherwise
        /// </summary>
        bool SaveSetting(SettingsLocation location, string name, string value);

        /// <summary>
        /// Remove the given setting if possible, return false otherwise
        /// </summary>
        bool RemoveSetting(SettingsLocation location, string name);

        /// <summary>
        /// Get the given setting value if possible, otherwise return null
        /// </summary>
        string GetSetting(string name);

        /// <summary>
        /// Get all settings (read only)
        /// </summary>
        IEnumerable<ClientSetting> GetAllSettings();

        /// <summary>
        /// Get the path to the settings file for the given location.
        /// </summary>
        string GetConfigurationPath(SettingsLocation location);
    }
}
