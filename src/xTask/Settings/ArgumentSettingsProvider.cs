// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using XTask.Utility;

    /// <summary>
    /// Argument provider that provides default arguments from IClientSettings
    /// </summary>
    public class ArgumentSettingsProvider : IArgumentProvider, IClientSettings
    {
        private IClientSettings clientSettings;
        private IArgumentProvider argumentProvider;
        private string settingsSection;

        protected ArgumentSettingsProvider(string settingsSection, IArgumentProvider argumentProvider, IClientSettings clientSettings)
            : base()
        {
            this.settingsSection = settingsSection;
            this.argumentProvider = argumentProvider;
            this.clientSettings = clientSettings;
        }

        public static ArgumentSettingsProvider Create(IArgumentProvider argumentProvider, string settingsSection = null)
        {
            settingsSection = settingsSection ?? Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + ".Defaults";
            ArgumentSettingsProvider settingsProvider = new ArgumentSettingsProvider(settingsSection, argumentProvider, ClientSettings.Create(settingsSection));
            return settingsProvider;
        }

        public bool SaveSetting(SettingsLocation location, string name, string value) { return this.clientSettings.SaveSetting(location, name, value); }
        public bool RemoveSetting(SettingsLocation location, string name) { return this.clientSettings.RemoveSetting(location, name); }
        public string GetSetting(string name) { return this.clientSettings.GetSetting(name); }
        public IEnumerable<ClientSetting> GetAllSettings() { return this.clientSettings.GetAllSettings(); }
        public string GetConfigurationPath(SettingsLocation location) { return this.clientSettings.GetConfigurationPath(location); }

        string IArgumentProvider.Target { get { return this.argumentProvider.Target; } }
        string IArgumentProvider.Command { get { return this.argumentProvider.Command; } }
        string[] IArgumentProvider.Targets { get { return this.argumentProvider.Targets; } }
        bool IArgumentProvider.HelpRequested { get { return this.argumentProvider.HelpRequested; } }

        // We don't consider deafult options to be set unless we're explicitly looking for them by name
        IReadOnlyDictionary<string, string> IArgumentProvider.Options { get { return this.argumentProvider.Options; } }

        T IArgumentProvider.GetOption<T>(params string[] optionNames)
        {
            if (optionNames == null || optionNames.Length == 0) { return default(T); }

            // Return the explict setting, if found
            object argumentValue = this.argumentProvider.GetOption<object>(optionNames);
            if (argumentValue != null)
            {
                return Types.ConvertType<T>(argumentValue);
            }

            // Don't have an explicit, look for a default
            string defaultSetting = this.GetSetting(optionNames[0]);
            if (!String.IsNullOrWhiteSpace(defaultSetting))
            {
                defaultSetting = Environment.ExpandEnvironmentVariables(defaultSetting);
            }

            return Types.ConvertType<T>(defaultSetting);
        }
    }
}
