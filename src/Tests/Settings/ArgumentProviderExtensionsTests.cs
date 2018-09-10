// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Settings
{
    using System;
    using FluentAssertions;
    using NSubstitute;
    using XTask.Systems.File;
    using XTask.Settings;
    using XTask.Utility;
    using Xunit;
    using System.IO;

    public class ArgumentProviderExtensionsTests
    {
        [Fact]
        public void GetDirectories_NullReturnsEmpty()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.Targets.Returns((string[])null);
            IFileService fileService = Substitute.For<IFileService>();

            arguments.GetDirectories(fileService).Should().BeEmpty();
        }

        [Fact]
        public void GetDirectories_EmptyReturnsEmpty()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.Targets.Returns(new string[0]);
            IFileService fileService = Substitute.For<IFileService>();

            arguments.GetDirectories(fileService).Should().BeEmpty();
        }

        [Fact]
        public void GetDirectories_NullFirstReturnsEmpty()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.Targets.Returns(new string[] { (string)null });
            IFileService fileService = Substitute.For<IFileService>();

            arguments.GetDirectories(fileService).Should().BeEmpty();
        }

        [Fact]
        public void GetDirectories_NotExistThrows()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.Targets.Returns(new string[] { "Foo" });
            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetFullPath("").ReturnsForAnyArgs(i => (string)i[0]);
            fileService.GetAttributes("Foo").Returns(x => { throw new FileNotFoundException(); });

            Action action = () => arguments.GetDirectories(fileService);
            action.Should().Throw<TaskArgumentException>();
        }

        [Fact]
        public void GetDirectories_Splits()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.Targets.Returns(new string[] { "Foo", "Bar;FooBar" });
            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetFullPath("").ReturnsForAnyArgs(i => (string)i[0]);
            fileService.GetAttributes("").ReturnsForAnyArgs(FileAttributes.Directory);

            arguments.GetDirectories(fileService).Should().BeEquivalentTo("Foo", "Bar", "FooBar");
        }

        [Fact]
        public void GetFilesFromArgument_NullReturnsEmpty()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>("Option").Returns((string)null);
            IFileService fileService = Substitute.For<IFileService>();

            arguments.GetFilesFromArgument(fileService, "Option").Should().BeEmpty();
        }

        [Fact]
        public void GetFilesFromArgument_BasicTest()
        {
            // Should split extensions across semicolons and trim leading asterisks
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>("Option").Returns("Bar;FooBar");
            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetFullPath("").ReturnsForAnyArgs(i => (string)i[0]);

            arguments.GetFilesFromArgument(fileService, "Option").Should().BeEquivalentTo("Bar", "FooBar");
        }

        [Fact]
        public void GetExtensionsFromArgument_NullReturnsEmpty()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>("Option").Returns((string)null);
            arguments.GetExtensionsFromArgument("Option").Should().BeEmpty();
        }

        [Fact]
        public void GetExtensionsFromArgument_BasicTest()
        {
            // Should split extensions across semicolons and trim leading asterisks
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>("Option").Returns("*Bar;FooBar");
            arguments.GetExtensionsFromArgument("Option").Should().BeEquivalentTo("Bar", "FooBar");
        }
    }
}
