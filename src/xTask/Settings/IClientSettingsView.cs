// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace XTask.Settings
{
    /// <summary>
    ///  Interface for modifying a client settings view.
    /// </summary>
    public interface IClientSettingsView
    {
        /// <summary>
        ///  Get the given setting value if possible, otherwise return <see langword="null"/>.
        /// </summary>
        string GetSetting(string name);

        /// <summary>
        ///  Save the given setting if possible, return <see langword="false"/> otherwise.
        /// </summary>
        bool SaveSetting(string name, string value);

        /// <summary>
        ///  Remove the given setting if possible, return <see langword="false"/> otherwise.
        /// </summary>
        bool RemoveSetting(string name);

        /// <summary>
        ///  Get all settings in the current view (read only).
        /// </summary>
        IEnumerable<ClientSetting> GetAllSettings();
    }
}
