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

    public class FlexDirectoryInfoTests
    {
        [Fact]
        public void DefaultEnumerate()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = cleaner.CreateTestDirectory();
                string filePath = cleaner.CreateTestFile("DefaultEnumerate", directoryPath);

                var directoryInfo = cleaner.FileService.GetPathInfo(directoryPath) as IDirectoryInformation;
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
                string directoryPath = cleaner.CreateTestDirectory();
                string filePath = cleaner.CreateTestFile("DefaultEnumerate", directoryPath);
                string fileName = Paths.GetFileOrDirectoryName(filePath);

                var directoryInfo = cleaner.FileService.GetPathInfo(cleaner.TempFolder) as IDirectoryInformation;
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
                string directoryPath = cleaner.CreateTestDirectory();
                string filePath = cleaner.CreateTestFile("EnumerateNestedFilteredFile", directoryPath);
                string fileName = Paths.GetFileOrDirectoryName(filePath);

                cleaner.FileService.AddAttributes(directoryPath, FileAttributes.Hidden);

                var directoryInfo = cleaner.FileService.GetPathInfo(cleaner.TempFolder) as IDirectoryInformation;
                directoryInfo.Should().NotBeNull();
                var files = directoryInfo.EnumerateChildren(ChildType.File, fileName, SearchOption.AllDirectories).ToArray();
                files.Should().HaveCount(0);
                cleaner.FileService.ClearAttributes(directoryPath, FileAttributes.Hidden);
            }
        }
    }
}
