// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using XTask.Systems.File;
using Xunit;

namespace XTask.Tests.Utility
{
    public class FileCleanerTests
    {
        public class TestFileCleaner : FileCleaner
        {
            public TestFileCleaner(string tempRootDirectoryName, IFileService fileServiceProvider)
                : base(tempRootDirectoryName, fileServiceProvider)
            {
            }

            public static string TestFlagFileName { get { return XTaskFlagFileName; } }
        }

        [Fact]
        public void FlagFileCreationTest()
        {
            IFileService fileServiceProvider = Substitute.For<IFileService>();
            MemoryStream memoryStream = new();
            fileServiceProvider.CreateFileStream("").ReturnsForAnyArgs(memoryStream);

            using FileCleaner cleaner = new("Test", fileServiceProvider);
            cleaner.TempFolder.Should().StartWith(Path.Combine(Path.GetTempPath(), "Test"));
            fileServiceProvider.Received(1).CreateFileStream(
                Path.Combine(cleaner.TempFolder, TestFileCleaner.TestFlagFileName),
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None);

            memoryStream.Position = 0;
            StreamReader reader = new(memoryStream);
            reader.ReadToEnd().Should().StartWith(XTaskStrings.FlagFileContent);
        }

        [Fact]
        public void CleanupHandlesDirectoryExceptions()
        {
            IFileService fileServiceProvider = Substitute.For<IFileService>();
            MemoryStream memoryStream = new();
            fileServiceProvider.CreateFileStream("").ReturnsForAnyArgs(memoryStream);
            IDirectoryInformation directoryInfo = Substitute.For<IDirectoryInformation>();
            IFileInformation fileInfo = Substitute.For<IFileInformation>();
            directoryInfo.EnumerateChildren().ReturnsForAnyArgs(new IFileInformation[] { fileInfo });
            fileServiceProvider.GetPathInfo("").ReturnsForAnyArgs(directoryInfo);

            using FileCleaner cleaner = new("Test", fileServiceProvider);
            fileServiceProvider.WhenForAnyArgs(f => f.DeleteDirectory("")).Do(a => { throw new Exception("TestException"); });
        }

        [Fact]
        public void CleanupHandlesFileExceptions()
        {
            IFileService fileServiceProvider = Substitute.For<IFileService>();
            MemoryStream memoryStream = new();
            fileServiceProvider.CreateFileStream("").ReturnsForAnyArgs(memoryStream);
            IDirectoryInformation directoryInfo = Substitute.For<IDirectoryInformation>();
            IFileInformation fileInfo = Substitute.For<IFileInformation>();
            directoryInfo.EnumerateChildren().ReturnsForAnyArgs(new IFileInformation[] { fileInfo });
            fileServiceProvider.GetPathInfo("").ReturnsForAnyArgs(directoryInfo);

            using FileCleaner cleaner = new("Test", fileServiceProvider);
            cleaner.TrackFile("Bar");
            fileServiceProvider.WhenForAnyArgs(f => f.DeleteFile("")).Do(a => { throw new Exception("TestException"); });
        }
    }
}
