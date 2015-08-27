// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using System.IO;
    using XTask.FileSystem;
    using XTask.Interop;
    using Xunit;
    using NSubstitute;

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

    }
}
