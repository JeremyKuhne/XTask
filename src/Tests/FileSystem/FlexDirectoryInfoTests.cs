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

    public class FlexDirectoryInfoTests
    {
        [Fact]
        public void DefaultEnumerate()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Directory.CreateDirectory(directoryPath);
                string filePath = Paths.Combine(directoryPath, Path.GetRandomFileName());
                File.WriteAllText(filePath, "DefaultEnumerate");

                FileService fileService = new FileService();
                var directoryInfo = fileService.GetPathInfo(directoryPath) as IDirectoryInformation;
                directoryInfo.Should().NotBeNull();
                var files = directoryInfo.EnumerateChildren().ToArray();
                files.Should().HaveCount(1);
                files[0].Path.Should().Be(filePath);
            }
        }

        [Fact]
        public void EnumerateNestedFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Directory.CreateDirectory(directoryPath);
                string fileName = Path.GetRandomFileName();
                string filePath = Paths.Combine(directoryPath, fileName);
                File.WriteAllText(filePath, "DefaultEnumerate");

                FileService fileService = new FileService();
                var directoryInfo = fileService.GetPathInfo(cleaner.TempFolder) as IDirectoryInformation;
                directoryInfo.Should().NotBeNull();
                var files = directoryInfo.EnumerateChildren(ChildType.File, fileName, SearchOption.AllDirectories).ToArray();
                files.Should().HaveCount(1);
                files[0].Path.Should().Be(filePath);
            }
        }

        [Fact]
        public void EnumerateNestedFilteredFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                var createdDirectory = Directory.CreateDirectory(directoryPath);
                string fileName = Path.GetRandomFileName();
                string filePath = Paths.Combine(directoryPath, fileName);
                File.WriteAllText(filePath, "DefaultEnumerate");
                createdDirectory.Attributes = createdDirectory.Attributes |= FileAttributes.Hidden;

                FileService fileService = new FileService();
                var directoryInfo = fileService.GetPathInfo(cleaner.TempFolder) as IDirectoryInformation;
                directoryInfo.Should().NotBeNull();
                var files = directoryInfo.EnumerateChildren(ChildType.File, fileName, SearchOption.AllDirectories).ToArray();
                files.Should().HaveCount(0);
                createdDirectory.Attributes = createdDirectory.Attributes &= ~FileAttributes.Hidden;
            }
        }
    }
}
