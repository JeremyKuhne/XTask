// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NSubstitute;
using System.IO;
using XTask.Systems.File;
using Xunit;

namespace XTask.Tests.Interop;

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
