// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using XTask.Settings;

namespace XTask.Tests.Settings;

public class ArgumentSettingsProviderTests
{
    public class TestArgumentSettingsProvider : ArgumentSettingsProvider
    {
        public TestArgumentSettingsProvider(string settingsSection, IArgumentProvider argumentProvider, IClientSettings clientSettings)
            : base (settingsSection, argumentProvider, clientSettings)
        {
        }
    }

    [Fact]
    public void GetArgumentGetsDefaultSetting()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
        settings.GetSetting("Foo").Returns("Bar");

        TestArgumentSettingsProvider provider = new("Section", arguments, settings);
        IArgumentProvider castArguments = provider;
        castArguments.GetOption<string>("Foo").Should().Be("Bar");
    }

    [Fact]
    public void GetArgumentGetsExplicitArgumentOverDefaultSetting()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
        arguments.GetOption<object>("Foo").Returns("NotBar");
        settings.GetSetting("Foo").Returns("Bar");

        TestArgumentSettingsProvider provider = new("Section", arguments, settings);
        IArgumentProvider castArguments = provider;
        castArguments.GetOption<string>("Foo").Should().Be("NotBar");
    }

    [Fact]
    public void IArgumentProviderPassesThrough()
    {
        IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
        arguments.Command.Returns("Command");
        arguments.HelpRequested.Returns(true);
        Dictionary<string, string> options = new();
        arguments.Options.Returns(options);
        string[] targets = { "Targets" };
        arguments.Targets.Returns(targets);
        arguments.Target.Returns("Target");

        TestArgumentSettingsProvider provider = new("Section", arguments, Substitute.For<IClientSettings>());
        IArgumentProvider castArguments = provider;
        castArguments.Target.Should().Be("Target");
        castArguments.Targets.Should().BeSameAs(targets);
        castArguments.Options.Should().BeSameAs(options);
        castArguments.HelpRequested.Should().BeTrue();
        castArguments.Command.Should().Be("Command");
    }

    [Fact]
    public void SaveSettingHitsClientSettings()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        TestArgumentSettingsProvider provider = new("Section", null, settings);
        provider.SaveSetting(SettingsLocation.Local, "Foo", "Bar");
        settings.Received(1).SaveSetting(SettingsLocation.Local, "Foo", "Bar");
    }

    [Fact]
    public void RemoveSettingHitsClientSettings()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        TestArgumentSettingsProvider provider = new("Section", null, settings);
        provider.RemoveSetting(SettingsLocation.Local, "Foo");
        settings.Received(1).RemoveSetting(SettingsLocation.Local, "Foo");
    }

    [Fact]
    public void GetSettingHitsClientSettings()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        TestArgumentSettingsProvider provider = new("Section", null, settings);
        provider.GetSetting("Foo");
        settings.Received(1).GetSetting("Foo");
    }

    [Fact]
    public void GetAllSettingsHitsClientSettings()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        TestArgumentSettingsProvider provider = new("Section", null, settings);
        provider.GetAllSettings();
        settings.Received(1).GetAllSettings();
    }

    [Fact]
    public void GetConfigurationPathHitsClientSettings()
    {
        IClientSettings settings = Substitute.For<IClientSettings>();
        TestArgumentSettingsProvider provider = new("Section", null, settings);
        provider.GetConfigurationPath(SettingsLocation.Local);
        settings.Received(1).GetConfigurationPath(SettingsLocation.Local);
    }
}
