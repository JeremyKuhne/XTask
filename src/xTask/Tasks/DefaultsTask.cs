// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using XTask.Logging;
using XTask.Settings;

namespace XTask.Tasks
{
    /// <summary>
    ///  Task for managing defaults.
    /// </summary>
    public class DefaultsTask : Task
    {
        private readonly string _applicationName;
        protected override string GeneralHelp
            => string.Format(CultureInfo.InvariantCulture, XTaskStrings.HelpDefaults, _applicationName);
        protected override string OptionDetails => XTaskStrings.HelpDefaultsOptions;

        public DefaultsTask(string applicationName) : base() => _applicationName = applicationName;

        protected override ExitCode ExecuteInternal()
        {
            IClientSettings clientSettings = Arguments as IClientSettings;
            SettingsLocation? location;

            if ((location = Arguments.GetOption<SettingsLocation?>(StandardOptions.Add)).HasValue)
            {
                return ChangeSetting(clientSettings, location.Value, ChangeType.Add);
            }
            else if ((location = Arguments.GetOption<SettingsLocation?>(StandardOptions.Remove)).HasValue)
            {
                return ChangeSetting(clientSettings, location.Value, ChangeType.Remove);
            }
            else
            {
                return ListSettings(clientSettings);
            }
        }

        protected virtual ExitCode ListSettings(IClientSettings clientSettings)
        {
            Table table = Table.Create(ColumnFormat.FromCount(2));
            table.HasHeader = true;
            table.AddRow(XTaskStrings.ConfigurationTypeColumn, XTaskStrings.ConfigurationLocationColumn);
            foreach (SettingsLocation location in new SettingsLocation[] { SettingsLocation.Local, SettingsLocation.Roaming, SettingsLocation.RunningExecutable })
            {
                table.AddRow(location.ToString(), clientSettings.GetConfigurationPath(location) ?? XTaskStrings.NoValue);
            }

            Loggers[LoggerType.Result].Write(table);
            Loggers[LoggerType.Result].WriteLine();

            List<ClientSetting> settings =
            (
                from setting in clientSettings.GetAllSettings()
                orderby setting.Name
                orderby setting.Location
                select setting
            ).ToList();

            Loggers[LoggerType.Result].WriteLine(XTaskStrings.DefaultsCount, settings.Count);
            table = Table.Create(ColumnFormat.FromCount(3));
            table.HasHeader = true;
            table.AddRow(XTaskStrings.DefaultsSettingColumnHeader, XTaskStrings.DefaultsLocationColumnHeader, XTaskStrings.DefaultsValueColumnHeader);

            foreach (ClientSetting setting in settings)
            {
                table.AddRow(setting.Name, setting.Location.ToString(), setting.Value.ToString());
            }

            Loggers[LoggerType.Result].Write(table);
            return ExitCode.Success;
        }

        public enum ChangeType
        {
            Add,
            Remove
        }

        protected virtual ExitCode ChangeSetting(IClientSettings clientSettings, SettingsLocation location, ChangeType changeType)
        {
            // Don't want to save defaults for options that apply directly to this command
            List<string> settingsToSkip = new();
            settingsToSkip.AddRange(StandardOptions.List);
            settingsToSkip.AddRange(StandardOptions.Add);
            settingsToSkip.AddRange(StandardOptions.Remove);

            foreach (var setting in Arguments.Options)
            {
                if (settingsToSkip.Contains(setting.Key, StringComparer.OrdinalIgnoreCase)) { continue; }
                bool success = false;
                switch (changeType)
                {
                    case ChangeType.Add:
                        Loggers[LoggerType.Status].Write(XTaskStrings.DefaultsSavingProgress, setting.Key);
                        success = clientSettings.SaveSetting(location, setting.Key, setting.Value);
                        break;
                    case ChangeType.Remove:
                        Loggers[LoggerType.Status].Write(XTaskStrings.DefaultsRemovingProgress, setting.Key);
                        success = clientSettings.RemoveSetting(location, setting.Key);
                        break;
                }

                Loggers[LoggerType.Status].WriteLine(success ? XTaskStrings.Succeeded : XTaskStrings.Failed);
            }

            return ExitCode.Success;
        }

        public override string Summary => XTaskStrings.DefaultsTaskSummary;
    }
}
