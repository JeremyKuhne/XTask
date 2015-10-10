// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using System;
    using System.IO;
    using FluentAssertions;
    using XTask.Systems.File;
    using Xunit;
    using Interop;
    using XTask.Systems.File.Concrete.Flex;
    using DotNet = XTask.Systems.File.Concrete.DotNet;
    using XTask.Interop;

    public class FlexFileServiceTests
    {
        [Fact]
        public void CreateDirectoryTest()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                FileService fileService = new FileService();
                fileService.CreateDirectory(directoryPath);

                File.Exists(directoryPath).Should().BeFalse();
                Directory.Exists(directoryPath).Should().BeTrue();
            }
        }

        [Fact]
        public void CreateNestedDirectoryTest()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                string secondDirectoryPath = Paths.Combine(directoryPath, Path.GetRandomFileName());
                FileService fileService = new FileService();
                fileService.CreateDirectory(secondDirectoryPath);

                File.Exists(directoryPath).Should().BeFalse();
                Directory.Exists(directoryPath).Should().BeTrue();
                File.Exists(secondDirectoryPath).Should().BeFalse();
                Directory.Exists(secondDirectoryPath).Should().BeTrue();
            }
        }

        [Fact]
        public void CreateLongPathNestedDirectoryTest()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 300);
                FileService fileService = new FileService();
                fileService.CreateDirectory(longPath);

                NativeMethods.FileManagement.FileExists(longPath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(longPath).Should().BeTrue();
            }
        }

        [Theory
            // InlineData(@" "),  // 5 Access is denied (UnauthorizedAccess)
            // InlineData(@"...") // 5 
            // InlineData(@" \"), // 123
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateStreamsWithDifficultNames(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                FileService fileService = new FileService();
                string filePath = Paths.Combine(cleaner.TempFolder, fileName);
                using (Stream fileStream = fileService.CreateFileStream(filePath, FileMode.CreateNew))
                {
                    fileStream.Should().NotBeNull();
                    fileService.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void CreateStream()
        {
            using (var cleaner = new TestFileCleaner())
            {
                FileService fileService = new FileService();
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                using (Stream fileStream = fileService.CreateFileStream(filePath, FileMode.CreateNew))
                {
                    fileStream.Should().NotBeNull();
                }
            }
        }

        [Fact]
        public void CreateLongPathStream()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 300);
                FileService fileService = new FileService();
                fileService.CreateDirectory(longPath);
                string filePath = Paths.Combine(longPath, Path.GetRandomFileName());
                using (Stream fileStream = fileService.CreateFileStream(filePath, FileMode.CreateNew))
                {
                    fileStream.Should().NotBeNull();
                }
            }
        }

        [Fact]
        public void WriteAndReadAlternateStreams()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Directory.CreateDirectory(directoryPath);

                FileService fileService = new FileService();
                string filePath = Paths.Combine(directoryPath, Path.GetRandomFileName());
                using (Stream fileStream = fileService.CreateFileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite)) { }

                for (int i = 0; i < 3; i++)
                {
                    string streamPath = $"{filePath}:Stream{i}:$DATA";
                    using (Stream fileStream = fileService.CreateFileStream(streamPath, FileMode.CreateNew, FileAccess.ReadWrite))
                    {
                        string testString = $"This is test string {i}.";
                        fileStream.Should().NotBeNull();
                        StreamWriter writer = new StreamWriter(fileStream);
                        writer.WriteLine(testString);
                        writer.Flush();
                        fileStream.Position = 0;
                        StreamReader reader = new StreamReader(fileStream);
                        string readLine = reader.ReadLine();
                        readLine.Should().Be(testString);
                    }
                }

                var directoryInfo = fileService.GetPathInfo(directoryPath) as IDirectoryInformation;
                directoryInfo.Should().NotBeNull();
                directoryInfo.EnumerateChildren().Should().HaveCount(1);
            }
        }

        [Theory
            InlineData("")
            InlineData(":MyStream:$DATA")
            InlineData("::$DATA")]
        public void WriteAndReadStream(string appendix)
        {
            using (var cleaner = new TestFileCleaner())
            {
                FileService fileService = new FileService();
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName() + appendix);
                using (Stream fileStream = fileService.CreateFileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    fileStream.Should().NotBeNull();
                    StreamWriter writer = new StreamWriter(fileStream);
                    writer.WriteLine("This is a test string.");
                    writer.Flush();
                    fileStream.Position = 0;
                    StreamReader reader = new StreamReader(fileStream);
                    string readLine = reader.ReadLine();
                    readLine.Should().Be("This is a test string.");
                }
            }
        }

        [Fact]
        public void WriteAndReadLongPathStream()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 300);
                FileService fileService = new FileService();
                fileService.CreateDirectory(longPath);
                string filePath = Paths.Combine(longPath, Path.GetRandomFileName());
                using (Stream fileStream = fileService.CreateFileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    fileStream.Should().NotBeNull();
                    StreamWriter writer = new StreamWriter(fileStream);
                    writer.WriteLine("This is a test string.");
                    writer.Flush();
                    fileStream.Position = 0;
                    StreamReader reader = new StreamReader(fileStream);
                    string readLine = reader.ReadLine();
                    readLine.Should().Be("This is a test string.");
                }
            }
        }

        [Theory
            InlineData("")
            InlineData(":MyStream:$DATA")
            InlineData("::$DATA")]
        public void OpenNonExistantStream(string appendix)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.GetTestPath() + appendix;
                Action action = () => new FileService().CreateFileStream(filePath, FileMode.Open);
                action.ShouldThrow<FileNotFoundException>();
            }
        }

        [Fact]
        public void FileInfoHasCanonicalPaths()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.GetTestPath() + "UPPER";
                cleaner.FileService.WriteAllText(filePath, "FileInfoHasCanonicalPaths");
                var info = new FileService().GetPathInfo(filePath.ToLowerInvariant());
                info.Path.Should().Be(filePath);
            }
        }
    }
}
