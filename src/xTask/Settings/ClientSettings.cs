// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System.Collections.Generic;
    using Services;
    using Systems.File;
    using Systems.Configuration;

    public class ClientSettings : IClientSettings
    {
        private Dictionary<SettingsLocation, IClientSettingsView> settingsViews = new Dictionary<SettingsLocation, IClientSettingsView>();
        private List<SettingsLocation> locationPriority = new List<SettingsLocation>();
        private IConfigurationManager configurationManager;
        private IFileService fileService;

        private ClientSettings(string settingsSection, IConfigurationManager configurationManager, IFileService fileService)
        {
            this.SettingsSection = settingsSection;
            this.configurationManager = configurationManager;
            this.fileService = fileService;
        }

        public string SettingsSection { get; private set; }

        public static ClientSettings Create(string settingsSection, IConfigurationManager configurationManager, IFileService fileService)
        {
            ClientSettings settings = new ClientSettings(settingsSection, configurationManager, fileService);
            settings.Initialize();
            return settings;
        }

        private void Initialize()
        {
            this.AddViews(SettingsLocation.ContainingExecutable, SettingsLocation.RunningExecutable, SettingsLocation.Roaming, SettingsLocation.Local);
        }

        private void AddViews(params SettingsLocation[] locations)
        {
            foreach (SettingsLocation location in locations)
            {
                IClientSettingsView view = ClientSettingsView.Create(this.SettingsSection, location, this.configurationManager, this.fileService);
                if (view != null)
                {
                    this.settingsViews.Add(location, ClientSettingsView.Create(this.SettingsSection, location, this.configurationManager, this.fileService));
                    this.locationPriority.Add(location);
                }
            }
        }

        public bool SaveSetting(SettingsLocation location, string name, string value)
        {
            if (!this.settingsViews.ContainsKey(location))
            {
                return false;
            }

            return this.settingsViews[location].SaveSetting(name, value);
        }

        public bool RemoveSetting(SettingsLocation location, string name)
        {
            if (!this.settingsViews.ContainsKey(location))
            {
                return false;
            }

            return this.settingsViews[location].RemoveSetting(name);
        }

        public string GetSetting(string name)
        {
            string value = null;
            foreach (SettingsLocation location in this.locationPriority)
            {
                value = this.settingsViews[location].GetSetting(name) ?? value;
            }
            return value;
        }

        public IEnumerable<ClientSetting> GetAllSettings()
        {
            List<ClientSetting> settings = new List<ClientSetting>();
            foreach (IClientSettingsView view in this.settingsViews.Values)
            {
                settings.AddRange(view.GetAllSettings());
            }

            return settings;
        }

        public string GetConfigurationPath(SettingsLocation location)
        {
            return this.GetConfigurationPath(location);
        }
    }
}
