// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Settings
{
    using System;
    using System.Configuration;
    using System.Xml;
    using FluentAssertions;
    using NSubstitute;
    using XTask.Systems.Configuration;
    using XTask.Systems.File;
    using XTask.Settings;
    using Xunit;
    using System.IO;

    public class ClientSettingsViewTests
    {
        private class TestClientSettingsView : ClientSettingsView
        {
            private TestClientSettingsView(string settingsSection, SettingsLocation settingsLocation) :
                base(settingsSection, settingsLocation) { }

            public static IConfigurationManager TestConfigurationManager
            {
                get { return ClientSettingsView.ConfigurationManager; }
                set { ClientSettingsView.ConfigurationManager = value; }
            }

            public static IFileService TestFileService
            {
                get { return ClientSettingsView.FileService.Value; }
                set { ClientSettingsView.FileService = new Lazy<IFileService>(() => value); }
            }

            public static IConfiguration TestGetConfiguration(ConfigurationUserLevel userLevel)
            {
                return ClientSettingsView.GetConfiguration(userLevel);
            }

            public static IConfiguration TestGetContainingConfigurationIfDifferent()
            {
                return ClientSettingsView.GetContainingConfigurationIfDifferent();
            }

            public static IConfiguration TestGetConfiguration(SettingsLocation location)
            {
                return ClientSettingsView.GetConfiguration(location);
            }

            public static XmlNode TestSerializeToXmlElement(string serializedValue)
            {
                return ClientSettingsView.SerializeToXmlElement(serializedValue);
            }
        }

        [Fact]
        public void GetConfigurationTest()
        {
            // We should get the expected configuration objects
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            IConfiguration noneConfiguration = Substitute.For<IConfiguration>();
            IConfiguration allOtherConfigurations = Substitute.For<IConfiguration>();
            allOtherConfigurations.FilePath.Returns("TestFilePath");
            configurationManager.OpenConfiguration("TestFilePath").Returns(allOtherConfigurations);

            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(
                x => (ConfigurationUserLevel)x[0] == ConfigurationUserLevel.None ? noneConfiguration : allOtherConfigurations);

            TestClientSettingsView.TestConfigurationManager = configurationManager;

            // None should return the none configuration, others should find the allOther via the TestFilePath lookup
            TestClientSettingsView.TestGetConfiguration(ConfigurationUserLevel.None).Should().Be(noneConfiguration);
            TestClientSettingsView.TestGetConfiguration(ConfigurationUserLevel.PerUserRoaming).Should().Be(allOtherConfigurations);
            TestClientSettingsView.TestGetConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).Should().Be(allOtherConfigurations);

            TestClientSettingsView.TestGetConfiguration(SettingsLocation.RunningExecutable).Should().Be(noneConfiguration);
            TestClientSettingsView.TestGetConfiguration(SettingsLocation.Executable).Should().Be(noneConfiguration);
            TestClientSettingsView.TestGetConfiguration(SettingsLocation.Local).Should().Be(allOtherConfigurations);
            TestClientSettingsView.TestGetConfiguration(SettingsLocation.Roaming).Should().Be(allOtherConfigurations);

            // This won't be true if running at the console
            // TestClientSettingsView.TestGetConfiguration(SettingsLocation.ContainingExecutable).Should().BeNull();

            // Now we set the file system to return a path exists
            IFileService testFileService = Substitute.For<IFileService>();
            testFileService.GetAttributes("").ReturnsForAnyArgs(FileAttributes.Normal);
            configurationManager.OpenConfiguration("").ReturnsForAnyArgs(allOtherConfigurations);
            TestClientSettingsView.TestFileService = testFileService;

            // Validate our direct path access route works as expected
            TestClientSettingsView.TestGetConfiguration(SettingsLocation.ContainingExecutable).Should().Be(allOtherConfigurations);
        }

        [Fact]
        public void GetConfigurationPathTest()
        {
            // Configuration path should return the given values in the configuration manager
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configuration.FilePath.Returns("TestFilePath");
            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(configuration);

            TestClientSettingsView.TestConfigurationManager = configurationManager;

            TestClientSettingsView.GetConfigurationPath(SettingsLocation.Executable).Should().Be("TestFilePath");
        }

        [Fact]
        public void InitializeCreatesSectionGroupTest()
        {
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(configuration);
            configuration.GetSectionGroup("userSettings").Returns((IConfigurationSectionGroup) => null);

            TestClientSettingsView.TestConfigurationManager = configurationManager;
            var testView = TestClientSettingsView.Create("Foo", SettingsLocation.Executable);
            configuration.Received().AddSectionGroup("userSettings");
        }

        [Fact]
        public void InitializeCreatesClientSettingsSection()
        {
            // Ensure initialization creates a client settings section if it doesn't have one
            IConfigurationSectionGroup sectionGroup = Substitute.For<IConfigurationSectionGroup>();
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configuration.GetSectionGroup("userSettings").Returns(sectionGroup);
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(configuration);

            TestClientSettingsView.TestConfigurationManager = configurationManager;
            var testView = TestClientSettingsView.Create("Foo", SettingsLocation.Executable);

            configuration.DidNotReceiveWithAnyArgs().AddSectionGroup("");
            sectionGroup.Received().Add("Foo", Arg.Any<ClientSettingsSection>());
        }

        [Fact]
        public void SerializeToXmlElement_Null()
        {
            // Null should be treated as empty string
            XmlNode node = TestClientSettingsView.TestSerializeToXmlElement(null);
            node.Should().NotBeNull();
            node.InnerXml.Should().Be(String.Empty);
        }

        [Fact]
        public void SerializeToXmlElement_Escaped()
        {
            // Special characters should be escaped
            XmlNode node = TestClientSettingsView.TestSerializeToXmlElement("<Foo>");
            node.Should().NotBeNull();
            node.InnerXml.Should().Be("&lt;Foo&gt;");
        }

        [Theory,
            InlineData(""),
            InlineData("Foo"),
            InlineData("You&Me"),
            InlineData("食べます"),
            InlineData("'Wow'"),
            InlineData("\"Wow\"")]
        public void RoundTripTest(string value)
        {
            // Ensure values round trip successfully
            IConfigurationSectionGroup sectionGroup = Substitute.For<IConfigurationSectionGroup>();
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configuration.GetSectionGroup("userSettings").Returns(sectionGroup);
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(configuration);

            TestClientSettingsView.TestConfigurationManager = configurationManager;
            var testView = TestClientSettingsView.Create("Foo", SettingsLocation.Executable);
            testView.SaveSetting("foo", value).Should().BeTrue();
            testView.GetSetting("foo").Should().Be(value);
        }
    }
}
