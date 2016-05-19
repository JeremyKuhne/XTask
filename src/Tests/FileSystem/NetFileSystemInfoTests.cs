﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.IO;
using XTask.Systems.File;
using XTask.Systems.File.Concrete.DotNet;
using Xunit;

namespace XTask.Tests.FileSystem
{
    public class NetFileSystemInfoTests
    {
        [Fact]
        public void CreateInfoForRootDrive()
        {
            string driveRoot = Paths.GetRoot(Path.GetTempPath());
            FileService fileService = new FileService();

            var info = fileService.GetPathInfo(driveRoot);
            info.Should().BeAssignableTo<IDirectoryInformation>();
            info.Exists.Should().BeTrue();
            info.Name.Should().Be(driveRoot);
            info.Path.Should().Be(driveRoot);
        }
    }
}
