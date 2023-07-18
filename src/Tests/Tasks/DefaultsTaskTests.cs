// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using XTask.Collections;
using XTask.Logging;
using XTask.Settings;
using XTask.Tasks;
using Xunit;

namespace XTask.Tests.Tasks;

public class DefaultsTaskTests
{
    [Fact]
    public void ListSettings_CountIsCorrect()
    {
        // Ensure we output the location table and all settings when listing settings

        // Set up loggers
        List<ITable> outputTables = new();
        ILogger logger = Substitute.For<ILogger>();
        logger.Write(Arg.Do<ITable>(x => outputTables.Add(x)));
        ILoggers loggers = Substitute.For<ILoggers>();
        loggers[Arg.Any<LoggerType>()].Returns(logger);

        // Set up to list settings
        ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
        interaction.Loggers.Returns(loggers);
        IArgumentProvider argumentProvider = Substitute.For<IArgumentProvider, IClientSettings>();
        argumentProvider.GetOption<SettingsLocation?>(StandardOptions.List).Returns(new SettingsLocation?(SettingsLocation.Local));
        interaction.Arguments.Returns(argumentProvider);

        // Prepare the configuration results
        ((IClientSettings)argumentProvider).GetConfigurationPath(SettingsLocation.Local).Returns("LocalPath");
        ((IClientSettings)argumentProvider).GetConfigurationPath(SettingsLocation.Roaming).Returns("RoamingPath");
        ((IClientSettings)argumentProvider).GetConfigurationPath(SettingsLocation.RunningExecutable).Returns("ExePath");

        ClientSetting[] settings =
        {
            new ClientSetting("foo", "one", SettingsLocation.Local),
            new ClientSetting("bar", "two", SettingsLocation.Roaming)
        };
        ((IClientSettings)argumentProvider).GetAllSettings().Returns(settings);

        DefaultsTask task = new("Foo");
        task.Execute(interaction).Should().Be(ExitCode.Success);

        outputTables.Count.Should().Be(2, "table for locations and table for settings");
        outputTables[0].Rows.Skip(1).ForEachDoOne(
            row => row.Should().Contain(SettingsLocation.Local.ToString(), "LocalPath"),
            row => row.Should().Contain(SettingsLocation.Roaming.ToString(), "RoamingPath"),
            row => row.Should().Contain(SettingsLocation.RunningExecutable.ToString(), "ExePath"));

        outputTables[1].Rows.Skip(1).ForEachDoOne(
            row => row.Should().Contain("foo", SettingsLocation.Local.ToString(), "one"),
            row => row.Should().Contain("bar", SettingsLocation.Roaming.ToString(), "two"));
    }

    [Fact]
    public void AddSetting_SkipOptions()
    {
        // Ensure we skip saving options that directly apply to the defaults task

        // Set up to add settings
        ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
        IArgumentProvider argumentProvider = Substitute.For<IArgumentProvider, IClientSettings>();
        argumentProvider.GetOption<SettingsLocation?>(StandardOptions.Add).Returns(new SettingsLocation?(SettingsLocation.Local));
        interaction.Arguments.Returns(argumentProvider);

        argumentProvider.Options.Returns(new Dictionary<string, string>
            {
                { StandardOptions.List[0], "one" },
                { StandardOptions.Add[0], "two" },
                { StandardOptions.Remove[0], "three" },
            });

        DefaultsTask task = new("Foo");
        task.Execute(interaction).Should().Be(ExitCode.Success);

        ((IClientSettings)argumentProvider).DidNotReceiveWithAnyArgs().SaveSetting(SettingsLocation.Local, "", "");
    }

    [Fact]
    public void AddSetting_Adds()
    {
        // Ensure actually add a setting when adding

        // Set up to add settings
        ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
        IArgumentProvider argumentProvider = Substitute.For<IArgumentProvider, IClientSettings>();
        argumentProvider.GetOption<SettingsLocation?>(StandardOptions.Add).Returns(new SettingsLocation?(SettingsLocation.Local));
        interaction.Arguments.Returns(argumentProvider);

        argumentProvider.Options.Returns(new Dictionary<string, string>
            {
                { "Boy", "Howdy" }
            });

        DefaultsTask task = new("Foo");
        task.Execute(interaction).Should().Be(ExitCode.Success);

        ((IClientSettings)argumentProvider).Received(1).SaveSetting(SettingsLocation.Local, "Boy", "Howdy");
    }

    [Fact]
    public void RemoveSetting_Removes()
    {
        // Ensure actually add a setting when adding

        // Set up to add settings
        ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
        IArgumentProvider argumentProvider = Substitute.For<IArgumentProvider, IClientSettings>();
        argumentProvider.GetOption<SettingsLocation?>(StandardOptions.Remove).Returns(new SettingsLocation?(SettingsLocation.Roaming));
        interaction.Arguments.Returns(argumentProvider);

        argumentProvider.Options.Returns(new Dictionary<string, string>
            {
                { "Boy", "Howdy" }
            });

        DefaultsTask task = new("Foo");
        task.Execute(interaction).Should().Be(ExitCode.Success);

        ((IClientSettings)argumentProvider).Received(1).RemoveSetting(SettingsLocation.Roaming, "Boy");
    }
}
