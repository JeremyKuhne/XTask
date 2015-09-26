// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using NSubstitute;
    using System;
    using System.IO;
    using XTask.Systems.File;
    using Xunit;

    public class FileServiceExtensionTests
    {
        [Fact]
        public void MakeReadOnlyWritable()
        {
            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetAttributes("foo").Returns(FileAttributes.Normal | FileAttributes.ReadOnly);
            fileService.MakeWritable("foo");
            fileService.Received(1).GetAttributes("foo");
            fileService.Received(1).SetAttributes("foo", FileAttributes.Normal);
        }

        [Fact]
        public void MakeWritableWritable()
        {
            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetAttributes("foo").Returns(FileAttributes.Normal);
            fileService.MakeWritable("foo");
            fileService.Received(1).GetAttributes("foo");
            fileService.ReceivedWithAnyArgs(0).SetAttributes("foo", FileAttributes.Normal);
        }

        [Fact]
        public void GetFileInfoThrowsForDirectory()
        {
            IFileService service = Substitute.For<IFileService>();
            IDirectoryInformation info = Substitute.For<IDirectoryInformation>();
            service.GetPathInfo("").ReturnsForAnyArgs(info);

            Action action = () => service.GetFileInfo("Foo");
            action.ShouldThrow<FileExistsException>();
        }

        [Fact]
        public void GetDirectoryInfoThrowsForFile()
        {
            IFileService service = Substitute.For<IFileService>();
            IFileInformation info = Substitute.For<IFileInformation>();
            service.GetPathInfo("").ReturnsForAnyArgs(info);

            Action action = () => service.GetDirectoryInfo("Foo");
            action.ShouldThrow<FileExistsException>();
        }
    }
}
