// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Interop;
    using XTask.Systems.File;
    using Support;
    using Xunit;

    public class FileManagementTests
    {
        [Theory
            InlineData("")]
        public void FullPathErrorCases(string value)
        {
            Action action = () => NativeMethods.FileManagement.GetFullPathName(value);
            action.ShouldThrow<IOException>();
        }

        [Theory
            // InlineData(@" "),  // 5 Access is denied (UnauthorizedAccess)
            // InlineData(@"...") // 5 
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateFileTests(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, fileName);
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, FileMode.Create, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Theory
            InlineData(@" "),
            InlineData(@"...")
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateFileExtendedTests(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.AddExtendedPrefix(Paths.Combine(cleaner.TempFolder, fileName), addIfUnderLegacyMaxPath: true);
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, FileMode.Create, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.FlushFileBuffers(handle);
                    NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void FileExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string fileName = Path.GetRandomFileName();
                string filePath = Paths.Combine(cleaner.TempFolder, fileName);
                File.WriteAllText(filePath, "FileExists");
                NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void FileNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                NativeMethods.FileManagement.FileExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void LongPathFileExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                cleaner.FileService.CreateDirectory(longPath);

                string filePath = cleaner.CreateTestFile("FileExists", longPath);

                NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void LongPathFileNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                string filePath = cleaner.GetTestPath();

                NativeMethods.FileManagement.FileExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void DirectoryExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = cleaner.CreateTestDirectory();

                NativeMethods.FileManagement.FileExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(directoryPath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(directoryPath).Should().BeTrue();
            }
        }

        [Fact]
        public void DirectoryNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());

                NativeMethods.FileManagement.FileExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(directoryPath).Should().BeFalse();
            }
        }

        [Fact]
        public void AttributesForNonExistantLongPath()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                FileAttributes attributes;
                NativeMethods.FileManagement.TryGetFileAttributes(longPath, out attributes).Should().BeFalse();
                attributes.Should().Be(NativeMethods.FileManagement.InvalidFileAttributes);
            }
        }

        [Fact]
        public void FinalPathNameFromPath()
        {
            string tempPath = Path.GetTempPath();
            string lowerTempPath = tempPath.ToLowerInvariant();
            tempPath.Should().NotBe(lowerTempPath);
            NativeMethods.FileManagement.GetFinalPathName(lowerTempPath, 0, false).Should().Be(Paths.RemoveTrailingSeparators(tempPath));
        }

        [Fact]
        public void CreateSymbolicLinkToFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("CreateSymbolicLinkToFile");
                string symbolicLink = cleaner.GetTestPath();
                Action action = () => NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);

                if (cleaner.ExtendedFileService.CanCreateSymbolicLinks())
                {
                    action();
                    var attributes = NativeMethods.FileManagement.GetFileAttributes(symbolicLink);
                    attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
                }
                else
                {
                    // Can't create links unless you have admin rights SE_CREATE_SYMBOLIC_LINK_NAME SeCreateSymbolicLinkPrivilege
                    action.ShouldThrow<IOException>().And.HResult.Should().Be(NativeErrorHelper.GetHResultForWindowsError(NativeMethods.WinError.ERROR_PRIVILEGE_NOT_HELD));
                }
            }
        }

        [Fact]
        public void CreateSymbolicLinkToLongPathFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("CreateSymbolicLinkToLongPathFile");
                string symbolicLink = cleaner.GetTestPath();
                Action action = () => NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);

                if (cleaner.ExtendedFileService.CanCreateSymbolicLinks())
                {
                    action();
                    var attributes = NativeMethods.FileManagement.GetFileAttributes(symbolicLink);
                    attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
                }
                else
                {
                    action.ShouldThrow<IOException>().And.HResult.Should().Be(NativeErrorHelper.GetHResultForWindowsError(NativeMethods.WinError.ERROR_PRIVILEGE_NOT_HELD));
                }
            }
        }

        [Fact]
        public void FileTypeOfFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                using (var testFile = NativeMethods.FileManagement.CreateFile(
                    cleaner.GetTestPath(),
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite,
                    FileMode.Create,
                    0))
                {
                    NativeMethods.FileManagement.GetFileType(testFile).Should().Be(NativeMethods.FileManagement.FileType.FILE_TYPE_DISK);
                }
            }
        }

        [Theory
            InlineData(@"C:\")
            InlineData(@"\\?\C:\")
            ]
        public void FindFirstFileHandlesRoots(string path)
        {
            NativeMethods.FileManagement.FindFirstFile(path, directoriesOnly: false, getAlternateName: false, returnNullIfNotFound: true).Should().BeNull();
        }
    }
}
