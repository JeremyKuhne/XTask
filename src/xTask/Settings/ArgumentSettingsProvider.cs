// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using XTask.Utility;
using XTask.Systems.Configuration;
using XTask.Systems.File;

namespace XTask.Settings;

/// <summary>
///  Argument provider that provides default arguments from IClientSettings.
/// </summary>
public class ArgumentSettingsProvider : IArgumentProvider, IClientSettings
{
    private readonly IClientSettings _clientSettings;
    private readonly IArgumentProvider _argumentProvider;
    private readonly string _settingsSection;

    protected ArgumentSettingsProvider(string settingsSection, IArgumentProvider argumentProvider, IClientSettings clientSettings)
        : base()
    {
        _settingsSection = settingsSection;
        _argumentProvider = argumentProvider;
        _clientSettings = clientSettings;
    }

    public static ArgumentSettingsProvider Create(
        IArgumentProvider argumentProvider,
        IConfigurationManager configurationManager,
        IFileService fileService,
        string settingsSection = null)
    {
        settingsSection ??= Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + ".Defaults";
        return new ArgumentSettingsProvider(
            settingsSection,
            argumentProvider,
            ClientSettings.Create(settingsSection, configurationManager, fileService));
    }

    public bool SaveSetting(SettingsLocation location, string name, string value)
        => _clientSettings.SaveSetting(location, name, value);
    public bool RemoveSetting(SettingsLocation location, string name)
        => _clientSettings.RemoveSetting(location, name);
    public string GetSetting(string name) => _clientSettings.GetSetting(name);
    public IEnumerable<ClientSetting> GetAllSettings() => _clientSettings.GetAllSettings();
    public string GetConfigurationPath(SettingsLocation location) => _clientSettings.GetConfigurationPath(location);

    public string Target => _argumentProvider.Target;
    public string Command => _argumentProvider.Command;
    public string[] Targets => _argumentProvider.Targets;
    public bool HelpRequested => _argumentProvider.HelpRequested;

    // We don't consider deafult options to be set unless we're explicitly looking for them by name
    public IReadOnlyDictionary<string, string> Options => _argumentProvider.Options;

    public T GetOption<T>(params string[] optionNames)
    {
        if (optionNames is null || optionNames.Length == 0) { return default; }

        // Return the explict setting, if found
        object argumentValue = _argumentProvider.GetOption<object>(optionNames);
        if (argumentValue is not null)
        {
            return Types.ConvertType<T>(argumentValue);
        }

        // Don't have an explicit, look for a default
        string defaultSetting = GetSetting(optionNames[0]);
        if (!string.IsNullOrWhiteSpace(defaultSetting))
        {
            defaultSetting = Environment.ExpandEnvironmentVariables(defaultSetting);
        }

        return Types.ConvertType<T>(defaultSetting);
    }
}
