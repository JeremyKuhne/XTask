// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System;
using XTask.Systems.File;
using XTask.Systems.File.Concrete.Flex;
using Xunit;
using NSubstitute;

namespace XTask.Tests.FileSystem
{
    public class CurrentDirectoryTests
    {
        [Fact]
        public void SetCurrentDirectoryThrowsOnRelative()
        {
            IExtendedFileService extendedFileService = Substitute.For<IExtendedFileService>();
            IFileService fileService = Substitute.For<IFileService>();

            fileService.GetFullPath("").ReturnsForAnyArgs(x => x[0]);
            fileService.GetAttributes("").ReturnsForAnyArgs(System.IO.FileAttributes.Directory);
            extendedFileService.GetVolumeName("").ReturnsForAnyArgs("TestRoot");

            CurrentDirectory cd = new(fileService, extendedFileService);
            Action action = () => cd.SetCurrentDirectory("a");
            action.Should().Throw<ArgumentException>();
        }
    }
}
