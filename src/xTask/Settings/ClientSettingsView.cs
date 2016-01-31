// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using XTask.Systems.Configuration;
    using XTask.Systems.File;
    using XTask.Utility;

    /// <summary>
    /// Standard IClientSettingsView for accessing .exe.config and user configs
    /// </summary>
    public class ClientSettingsView : IClientSettingsView
    {
        private const string UserSettingsGroupName = "userSettings";

        private IConfiguration _configuration;
        private ClientSettingsSection _clientSettings;

        protected IConfigurationManager ConfigurationManager{ get; private set; }
        protected IFileService FileService { get; private set; }

        protected ClientSettingsView(string settingsSection, SettingsLocation settingsLocation, IConfigurationManager configurationManager, IFileService fileService)
        {
            ConfigurationManager = configurationManager;
            FileService = fileService;
            SettingsSection = settingsSection;
            SettingsLocation = settingsLocation;
        }

        public string SettingsSection { get; private set; }
        public SettingsLocation SettingsLocation { get; private set; }

        public static IClientSettingsView Create(string settingsSection, SettingsLocation settingsLocation, IConfigurationManager configurationManager, IFileService fileService)
        {
            try
            {
                ClientSettingsView view = new ClientSettingsView(settingsSection, settingsLocation, configurationManager, fileService);
                if (!view.Initialize())
                {
                    return null;
                }
                return view;
            }
            catch (Exception e)
            {
                // We don't have rights most likely, go ahead and return null
                Debug.WriteLine("Could not create settings for '{0}': {1}", settingsLocation, e.Message);
                return null;
            }
        }

        protected IConfiguration GetConfiguration(ConfigurationUserLevel userLevel)
        {
            // Configuration flows from Machine.config -> exe.config -> roaming user.config -> local user.config with
            // latter definitions trumping earlier (e.g. local trumps roaming, which trumps exe, etc.).
            //
            // Opening configuration other than None will provide a combined view of with roaming or roaming and local.
            // As we want to handle the consolodation ourselves we need to open twice. Once to get the actual user config
            // path, then again with the path explicitly specified with "None" for our user level. (Values are lazily
            // loaded so this isn't a terrible perf issue.)
            IConfiguration configuration = ConfigurationManager.OpenConfiguration(userLevel);

            if (userLevel == ConfigurationUserLevel.None)
            {
                return configuration;
            }
            else
            {
                return ConfigurationManager.OpenConfiguration(configuration.FilePath);
            }
        }

        protected IConfiguration GetContainingConfigurationIfDifferent()
        {
            IConfiguration configuration = GetConfiguration(ConfigurationUserLevel.None);

            // Only create this type if we don't match
            string codeBase = typeof(ClientSettingsView).Assembly.CodeBase;
            Uri codeBaseUri;
            if (!Uri.TryCreate(codeBase, UriKind.Absolute, out codeBaseUri) || !codeBaseUri.IsFile) { return null; }

            string assemblyConfig = codeBaseUri.LocalPath + ".config";
            if (!String.Equals(configuration.FilePath, assemblyConfig, StringComparison.OrdinalIgnoreCase)
                && FileService.FileExists(assemblyConfig))
            {
                // We don't match and exist, go ahead and try to create
                return ConfigurationManager.OpenConfiguration(assemblyConfig);
            }

            return null;
        }

        protected IConfiguration GetConfiguration(SettingsLocation location)
        {
            switch (location)
            {
                case SettingsLocation.Local:
                    return GetConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                case SettingsLocation.Roaming:
                    return GetConfiguration(ConfigurationUserLevel.PerUserRoaming);
                case SettingsLocation.RunningExecutable:
                    return GetConfiguration(ConfigurationUserLevel.None);
                case SettingsLocation.ContainingExecutable:
                    return GetContainingConfigurationIfDifferent();
            }
            return null;
        }

        /// <summary>
        /// Gets the file path for the given location's configuration file
        /// </summary>
        public string GetConfigurationPath(SettingsLocation location)
        {
            IConfiguration configuration = GetConfiguration(location);
            if (configuration != null)
            {
                return configuration.FilePath;
            }
            else
            {
                return null;
            }
        }

        private bool Initialize()
        {
            IConfiguration configuration = GetConfiguration(SettingsLocation);

            if (configuration == null) { return false; }

            _configuration = configuration;
            IConfigurationSectionGroup userGroup = this._configuration.GetSectionGroup(UserSettingsGroupName);
            if (userGroup == null)
            {
                userGroup = configuration.AddSectionGroup(UserSettingsGroupName);
            }

            ClientSettingsSection clientSettings = userGroup.Get(SettingsSection) as ClientSettingsSection;
 
            if (clientSettings == null)
            {
                clientSettings = new ClientSettingsSection();
                clientSettings.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                clientSettings.SectionInformation.RequirePermission = false;
                userGroup.Add(SettingsSection, clientSettings);
            }

            _clientSettings = clientSettings;
            return true;
        }

        public string GetSetting(string name)
        {
            IStringProperty property = SettingToProperty(GetSettingInternal(name));
            return property == null ? null : property.Value;
        }

        public IEnumerable<ClientSetting> GetAllSettings()
        {
            return _clientSettings.Settings.OfType<SettingElement>().Select(SettingToClientSetting).ToList();
        }

        public bool SaveSetting(string name, string value)
        {
            try
            {
                SettingElement element = GetSettingInternal(name);
                XmlNode valueXml = SerializeToXmlElement(value);
                if (element == null)
                {
                    element = new SettingElement(name, SettingsSerializeAs.String);
                    _clientSettings.Settings.Add(element);
                }

                element.Value.ValueXml = SerializeToXmlElement(value);
                _configuration.Save();
                return true;
            }
            catch (Exception e)
            {
                // We don't have rights most likely, go ahead and fail gracefully
                Debug.WriteLine("Could not save setting for '{0}': {1}", SettingsLocation, e.Message);
                return false;
            }
        }

        public bool RemoveSetting(string name)
        {
            try
            {
                SettingElement element = GetSettingInternal(name);
                if (element == null) { return true; }

                _clientSettings.Settings.Remove(element);
                _configuration.Save();
                return true;
            }
            catch (Exception e)
            {
                // We don't have rights most likely, go ahead and fail gracefully
                Debug.WriteLine("Could not remove setting for '{0}': {1}", SettingsLocation, e.Message);
                return false;
            }
        }

        private ClientSetting SettingToClientSetting(SettingElement setting)
        {
            IStringProperty property = SettingToProperty(setting);

            if (property == null)
            {
                Debug.Fail("Shouldn't be converting a null element to a ClientSetting.");
                return null;
            }
            else
            {
                return new ClientSetting(property.Name, property.Value, SettingsLocation);
            }
        }

        private static IStringProperty SettingToProperty(SettingElement setting)
        {
            if (setting == null) { return null; }
            return new StringProperty(setting.Name, XmlEscaper.Unescape(setting.Value.ValueXml.InnerXml));
        }

        private SettingElement GetSettingInternal(string name)
        {
            foreach (SettingElement setting in _clientSettings.Settings)
            {
                if (string.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return setting;
                }
            }

            return null;
        }

        protected static XmlNode SerializeToXmlElement(string serializedValue)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement valueXml = doc.CreateElement("value");

            if (serializedValue == null)
            {
                serializedValue = string.Empty;
            }

            // We need to escape string serialized values
            serializedValue = XmlEscaper.Escape(serializedValue);
            valueXml.InnerXml = serializedValue;

            // Hack to remove the XmlDeclaration that the XmlSerializer adds.
            XmlNode unwanted = null;
            foreach (XmlNode child in valueXml.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.XmlDeclaration)
                {
                    unwanted = child;
                    break;
                }
            }

            if (unwanted != null)
            {
                valueXml.RemoveChild(unwanted);
            }

            return valueXml;
        }
    }
}
