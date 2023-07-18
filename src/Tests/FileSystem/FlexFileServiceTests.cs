// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NSubstitute;
using System;
using System.IO;
using XTask.Systems.File;
using XTask.Systems.File.Concrete.Flex;
using XTask.Tests.Support;
using Xunit;

namespace XTask.Tests.FileSystem;

public class FlexFileServiceTests
{
    [Fact]
    public void CreateDirectoryTest()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
        cleaner.FileService.CreateDirectory(directoryPath);

        File.Exists(directoryPath).Should().BeFalse();
        Directory.Exists(directoryPath).Should().BeTrue();
    }

    [Fact]
    public void CreateNestedDirectoryTest()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
        string secondDirectoryPath = Paths.Combine(directoryPath, Path.GetRandomFileName());
        cleaner.FileService.CreateDirectory(secondDirectoryPath);

        File.Exists(directoryPath).Should().BeFalse();
        Directory.Exists(directoryPath).Should().BeTrue();
        File.Exists(secondDirectoryPath).Should().BeFalse();
        Directory.Exists(secondDirectoryPath).Should().BeTrue();
    }

    [Fact]
    public void CreateLongPathNestedDirectoryTest()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 300);
        cleaner.FileService.CreateDirectory(longPath);

        cleaner.FileService.FileExists(longPath).Should().BeFalse();
        cleaner.FileService.DirectoryExists(longPath).Should().BeTrue();
    }

    [Theory,
        // InlineData(@" "),  // 5 Access is denied (UnauthorizedAccess)
        // InlineData(@"...") // 5 
        // InlineData(@" \"), // 123
        InlineData(@"A "),
        InlineData(@"A.")
        ]
    public void CreateStreamsWithDifficultNames(string fileName)
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string filePath = Paths.Combine(cleaner.TempFolder, fileName);
        using Stream fileStream = cleaner.FileService.CreateFileStream(filePath, FileMode.CreateNew);
        fileStream.Should().NotBeNull();
        cleaner.FileService.FileExists(filePath).Should().BeTrue();
    }

    [Fact]
    public void CreateStream()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
        using Stream fileStream = cleaner.FileService.CreateFileStream(filePath, FileMode.CreateNew);
        fileStream.Should().NotBeNull();
    }

    [Fact]
    public void CreateLongPathStream()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 300);
        cleaner.FileService.CreateDirectory(longPath);

        string filePath = Paths.Combine(longPath, Path.GetRandomFileName());
        using Stream fileStream = cleaner.FileService.CreateFileStream(filePath, FileMode.CreateNew);
        fileStream.Should().NotBeNull();
    }

    [Fact]
    public void WriteAndReadAlternateStreams()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
        Directory.CreateDirectory(directoryPath);

        string filePath = Paths.Combine(directoryPath, Path.GetRandomFileName());
        using (Stream fileStream = cleaner.FileService.CreateFileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite)) { }

        for (int i = 0; i < 3; i++)
        {
            string streamPath = $"{filePath}:Stream{i}:$DATA";
            using Stream fileStream = cleaner.FileService.CreateFileStream(streamPath, FileMode.CreateNew, FileAccess.ReadWrite);
            string testString = $"This is test string {i}.";
            fileStream.Should().NotBeNull();
            StreamWriter writer = new(fileStream);
            writer.WriteLine(testString);
            writer.Flush();
            fileStream.Position = 0;
            StreamReader reader = new(fileStream);
            string readLine = reader.ReadLine();
            readLine.Should().Be(testString);
        }

        var directoryInfo = cleaner.FileService.GetPathInfo(directoryPath) as IDirectoryInformation;
        directoryInfo.Should().NotBeNull();
        directoryInfo.EnumerateChildren().Should().HaveCount(1);
    }

    [Theory,
        InlineData(""),
        InlineData(":MyStream:$DATA"),
        InlineData("::$DATA")]
    public void WriteAndReadStream(string appendix)
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName() + appendix);
        using Stream fileStream = cleaner.FileService.CreateFileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
        fileStream.Should().NotBeNull();
        StreamWriter writer = new(fileStream);
        writer.WriteLine("This is a test string.");
        writer.Flush();
        fileStream.Position = 0;
        StreamReader reader = new(fileStream);
        string readLine = reader.ReadLine();
        readLine.Should().Be("This is a test string.");
    }

    [Fact]
    public void WriteAndReadLongPathStream()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 300);

        cleaner.FileService.CreateDirectory(longPath);
        string filePath = Paths.Combine(longPath, Path.GetRandomFileName());
        using Stream fileStream = cleaner.FileService.CreateFileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
        fileStream.Should().NotBeNull();
        StreamWriter writer = new(fileStream);
        writer.WriteLine("This is a test string.");
        writer.Flush();
        fileStream.Position = 0;
        StreamReader reader = new(fileStream);
        string readLine = reader.ReadLine();
        readLine.Should().Be("This is a test string.");
    }

    [Theory,
        InlineData(""),
        InlineData(":MyStream:$DATA"),
        InlineData("::$DATA")]
    public void OpenNonExistantStream(string appendix)
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string filePath = cleaner.GetTestPath() + appendix;
        Action action = () => cleaner.FileService.CreateFileStream(filePath, FileMode.Open);
        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void FileInfoHasCanonicalPaths()
    {
        using var cleaner = new TestFileCleaner(useDotNet: false);
        string filePath = cleaner.GetTestPath() + "UPPER";
        cleaner.FileService.WriteAllText(filePath, "FileInfoHasCanonicalPaths");
        var info = cleaner.FileService.GetPathInfo(filePath.ToLowerInvariant());
        info.Path.Should().Be(filePath);
    }

    [Theory,
        InlineData(@"a", @"C:\b", @"C:\b\a"),
        InlineData(@"C:a", @"C:\b", @"C:\b\a"),
        InlineData(@"C:a", @"D:\b", @"C:\Users\a"),
        InlineData(@"C:\a\b", @"C:\b", @"C:\a\b"),
        InlineData(@"a", @"D:\b", @"D:\b\a"),
        InlineData(@"D:a", @"D:\b", @"D:\b\a"),
        InlineData(@"D:a", @"C:\b", @"D:\a")
        ]
    public void GetFullPathWithBasePathTests(string path, string basePath, string expected)
    {
        IExtendedFileService extendedFileService = Substitute.For<IExtendedFileService>();
        extendedFileService.GetVolumeName(@"C:\").Returns("TestCVolumeName");
        extendedFileService.GetVolumeName(@"D:\").Returns("TestDVolumeName");

        var fileService = new FileService(extendedFileService, initialCurrentDirectory: @"C:\Users");
        fileService.GetFullPath(path, basePath).Should().Be(expected);
    }
}
