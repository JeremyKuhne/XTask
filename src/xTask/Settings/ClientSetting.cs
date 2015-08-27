// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    public class ClientSetting : StringProperty
    {
        public SettingsLocation Location { get; private set; }

        public ClientSetting(string name, string value, SettingsLocation location)
            : base (name, value)
        {
            this.Location = location;
        }
    }
}
