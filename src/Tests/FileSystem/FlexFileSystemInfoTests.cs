// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using System;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using XTask.FileSystem;
    using Xunit;
    using Interop;
    using XTask.FileSystem.Concrete.Flex;
    using DotNet = XTask.FileSystem.Concrete.DotNet;
    using XTask.Interop;

    public class FlexFileSystemInfoTests
    {
        [Theory
            InlineData("")
            InlineData(Paths.ExtendedPathPrefix)]
        public void CreateInfoForRootDrive(string prefix)
        {
            string driveRoot = prefix + Paths.GetPathRoot(Path.GetTempPath());
            FileService fileService = new FileService();

            var info = fileService.GetPathInfo(driveRoot);
            info.Should().BeAssignableTo<IDirectoryInformation>();
            info.Exists.Should().BeTrue();
            info.Name.Should().Be(driveRoot);
            info.Path.Should().Be(driveRoot);
        }
    }
}
