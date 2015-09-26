// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Interop;
    using Xunit;
    using NSubstitute;
    using Systems.File;

    public class FileSystemInformationExtensions
    {
        [Fact]
        public void FileInfoReturnsNoFileChildren()
        {
            IFileInformation fileInfo = Substitute.For<IFileInformation>();
            fileInfo.EnumerateFiles().Should().BeEmpty();
        }

        [Fact]
        public void FileInfoReturnsNoDirectoryChildren()
        {
            IFileInformation fileInfo = Substitute.For<IFileInformation>();
            fileInfo.EnumerateFiles().Should().BeEmpty();
        }

        [Fact]
        public void EnumerateFilesPassesCorrectArguments()
        {
            IDirectoryInformation info = Substitute.For<IDirectoryInformation>();
            info.EnumerateFiles("foo", SearchOption.AllDirectories, 0);
            info.Received(1).EnumerateChildren(ChildType.File, "foo", SearchOption.AllDirectories, 0);
        }

        [Fact]
        public void EnumerateDirectoriesPassesCorrectArguments()
        {
            IDirectoryInformation info = Substitute.For<IDirectoryInformation>();
            info.EnumerateDirectories("foo", SearchOption.AllDirectories, 0);
            info.Received(1).EnumerateChildren(ChildType.Directory, "foo", SearchOption.AllDirectories, 0);
        }
    }
}
