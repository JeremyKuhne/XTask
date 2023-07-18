// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NSubstitute;
using System.Configuration;
using System.IO;
using System.Xml;
using XTask.Settings;
using XTask.Systems.Configuration;
using XTask.Systems.File;
using Xunit;

namespace XTask.Tests.Settings
{
    public class ClientSettingsViewTests
    {
        private class TestClientSettingsView : ClientSettingsView
        {
            public TestClientSettingsView(
                IConfigurationManager configurationManager,
                IFileService fileService,
                string settingsSection = "testsettings",
                SettingsLocation settingsLocation = SettingsLocation.Executable) :
                base(settingsSection, settingsLocation, configurationManager, fileService) { }

            public IConfiguration TestGetConfiguration(ConfigurationUserLevel userLevel)
            {
                return GetConfiguration(userLevel);
            }

            public IConfiguration TestGetContainingConfigurationIfDifferent()
            {
                return GetContainingConfigurationIfDifferent();
            }

            public IConfiguration TestGetConfiguration(SettingsLocation location)
            {
                return GetConfiguration(location);
            }

            public static XmlNode TestSerializeToXmlElement(string serializedValue)
            {
                return SerializeToXmlElement(serializedValue);
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

            // Now we set the file system to return a path exists
            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetAttributes("").ReturnsForAnyArgs(FileAttributes.Normal);
            configurationManager.OpenConfiguration("").ReturnsForAnyArgs(allOtherConfigurations);


            TestClientSettingsView view = new(configurationManager, fileService);

            // None should return the none configuration, others should find the allOther via the TestFilePath lookup
            view.TestGetConfiguration(ConfigurationUserLevel.None).Should().Be(noneConfiguration);
            view.TestGetConfiguration(ConfigurationUserLevel.PerUserRoaming).Should().Be(allOtherConfigurations);
            view.TestGetConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).Should().Be(allOtherConfigurations);

            view.TestGetConfiguration(SettingsLocation.RunningExecutable).Should().Be(noneConfiguration);
            view.TestGetConfiguration(SettingsLocation.Executable).Should().Be(noneConfiguration);
            view.TestGetConfiguration(SettingsLocation.Local).Should().Be(allOtherConfigurations);
            view.TestGetConfiguration(SettingsLocation.Roaming).Should().Be(allOtherConfigurations);

            // This won't be true if running at the console
            // TestClientSettingsView.TestGetConfiguration(SettingsLocation.ContainingExecutable).Should().BeNull();

            // Validate our direct path access route works as expected
            view.TestGetConfiguration(SettingsLocation.ContainingExecutable).Should().Be(allOtherConfigurations);
        }

        [Fact]
        public void GetConfigurationPathTest()
        {
            // Configuration path should return the given values in the configuration manager
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configuration.FilePath.Returns("TestFilePath");
            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(configuration);

            TestClientSettingsView view = new(configurationManager, null);

            view.GetConfigurationPath(SettingsLocation.Executable).Should().Be("TestFilePath");
        }

        [Fact]
        public void InitializeCreatesSectionGroupTest()
        {
            IConfigurationManager configurationManager = Substitute.For<IConfigurationManager>();
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configurationManager.OpenConfiguration(ConfigurationUserLevel.None).ReturnsForAnyArgs(configuration);
            configuration.GetSectionGroup("userSettings").Returns((IConfigurationSectionGroup) => null);

            var testView = ClientSettingsView.Create("Foo", SettingsLocation.Executable, configurationManager, null);
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
            _ = ClientSettingsView.Create("Foo", SettingsLocation.Executable, configurationManager, null);

            configuration.DidNotReceiveWithAnyArgs().AddSectionGroup("");
            sectionGroup.Received().Add("Foo", Arg.Any<ClientSettingsSection>());
        }

        [Fact]
        public void SerializeToXmlElement_Null()
        {
            // Null should be treated as empty string
            XmlNode node = TestClientSettingsView.TestSerializeToXmlElement(null);
            node.Should().NotBeNull();
            node.InnerXml.Should().Be(string.Empty);
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

            var testView = ClientSettingsView.Create("Foo", SettingsLocation.Executable, configurationManager, null);
            testView.SaveSetting("foo", value).Should().BeTrue();
            testView.GetSetting("foo").Should().Be(value);
        }
    }
}
