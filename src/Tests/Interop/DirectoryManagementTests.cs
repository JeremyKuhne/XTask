// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Interop;
    using Support;
    using Xunit;

    public class DirectoryManagementTests
    {
        [Fact]
        public void CreateDirectoryTest()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Path.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                NativeMethods.DirectoryManagement.CreateDirectory(directoryPath);

                File.Exists(directoryPath).Should().BeFalse();
                Directory.Exists(directoryPath).Should().BeTrue();
            }
        }

        [Fact]
        public void SetDirectoryTest()
        {
            using (var cleaner = new TestFileCleaner())
            using (new TempCurrentDirectory())
            {
                NativeMethods.DirectoryManagement.SetCurrentDirectory(cleaner.TempFolder);
                Directory.GetCurrentDirectory().Should().Be(cleaner.TempFolder);
            }
        }

        [Fact]
        public void SetDirectoryToNonExistant()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Path.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Action action = () => NativeMethods.DirectoryManagement.SetCurrentDirectory(directoryPath);
                action.ShouldThrow<FileNotFoundException>();
            }
        }

        [Fact]
        public void GetDirectoryTest()
        {
            using (var cleaner = new TestFileCleaner())
            using (new TempCurrentDirectory())
            {
                Directory.SetCurrentDirectory(cleaner.TempFolder);
                NativeMethods.DirectoryManagement.GetCurrentDirectory().Should().Be(cleaner.TempFolder);
            }
        }
    }
}
