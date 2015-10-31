// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using FluentAssertions;
    using System.IO;
    using Systems.File;
    using Systems.File.Concrete.Flex;
    using Systems.File.Concrete;
    using Xunit;

    public class FlexFileSystemInfoTests
    {
        [Theory
            InlineData("")
            InlineData(Paths.ExtendedPathPrefix)]
        public void CreateInfoForRootDrive(string prefix)
        {
            string driveRoot = prefix + Paths.GetRoot(Path.GetTempPath());
            FileService fileService = new FileService(new ExtendedFileService());

            var info = fileService.GetPathInfo(driveRoot);
            info.Should().BeAssignableTo<IDirectoryInformation>();
            info.Exists.Should().BeTrue();
            info.Name.Should().Be(driveRoot);
            info.Path.Should().Be(driveRoot);
        }
    }
}
