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
    using Services;
    using XTask.Utility;
    using Systems.Configuration;
    using Systems.File;

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

        public static ArgumentSettingsProvider Create(IArgumentProvider argumentProvider, IConfigurationManager configurationManager, IFileService fileService, string settingsSection = null)
        {
            settingsSection = settingsSection ?? Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + ".Defaults";
            ArgumentSettingsProvider settingsProvider = new ArgumentSettingsProvider(settingsSection, argumentProvider, ClientSettings.Create(settingsSection, configurationManager, fileService));
            return settingsProvider;
        }

        public bool SaveSetting(SettingsLocation location, string name, string value) { return this.clientSettings.SaveSetting(location, name, value); }
        public bool RemoveSetting(SettingsLocation location, string name) { return this.clientSettings.RemoveSetting(location, name); }
        public string GetSetting(string name) { return this.clientSettings.GetSetting(name); }
        public IEnumerable<ClientSetting> GetAllSettings() { return this.clientSettings.GetAllSettings(); }
        public string GetConfigurationPath(SettingsLocation location) { return this.clientSettings.GetConfigurationPath(location); }

        public string Target { get { return this.argumentProvider.Target; } }
        public string Command { get { return this.argumentProvider.Command; } }
        public string[] Targets { get { return this.argumentProvider.Targets; } }
        public bool HelpRequested { get { return this.argumentProvider.HelpRequested; } }

        // We don't consider deafult options to be set unless we're explicitly looking for them by name
        public IReadOnlyDictionary<string, string> Options { get { return this.argumentProvider.Options; } }

        public T GetOption<T>(params string[] optionNames)
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
