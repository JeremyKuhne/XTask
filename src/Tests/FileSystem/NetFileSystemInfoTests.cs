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
    using XTask.Systems.File;
    using Xunit;
    using Interop;
    using XTask.Systems.File.Concrete.DotNet;
    using XTask.Interop;

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
