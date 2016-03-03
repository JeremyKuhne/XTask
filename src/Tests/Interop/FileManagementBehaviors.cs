// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using Support;
    using System;
    using System.IO;
    using Systems.File.Concrete;
    using XTask.Interop;
    using XTask.Systems.File;
    using Xunit;

    /// <summary>
    /// Tests to validate assertions about Win32 FileManagement API behaviors
    /// </summary>
    public class FileManagementBehaviors
    {
        [Theory
            Trait("Environment", "CurrentDirectory")
            InlineData(@"C:", @"C:\Users")
            InlineData(@"C", @"D:\C")
            ]
        public void ValidateKnownRelativeBehaviors(string value, string expected)
        {
            // Set the current directory to D: and the hidden env for C:'s last current directory
            NativeMethods.SetEnvironmentVariable(@"=C:", @"C:\Users");
            using (new TempCurrentDirectory(@"D:\"))
            {
                NativeMethods.FileManagement.GetFullPathName(value).Should().Be(expected);
            }
        }

        [Theory
            // Basic dot space handling
            InlineData(@"C:\", @"C:\")
            InlineData(@"C:\ ", @"C:\")
            InlineData(@"C:\.", @"C:\")
            InlineData(@"C:\..", @"C:\")
            InlineData(@"C:\...", @"C:\")
            InlineData(@"C:\ .", @"C:\")
            InlineData(@"C:\ ..", @"C:\")
            InlineData(@"C:\ ...", @"C:\")
            InlineData(@"C:\. ", @"C:\")
            InlineData(@"C:\.. ", @"C:\")
            InlineData(@"C:\... ", @"C:\")
            InlineData(@"C:\.\", @"C:\")
            InlineData(@"C:\..\", @"C:\")
            InlineData(@"C:\...\", @"C:\...\")
            InlineData(@"C:\ \", @"C:\ \")
            InlineData(@"C:\ .\", @"C:\ \")
            InlineData(@"C:\ ..\", @"C:\ ..\")
            InlineData(@"C:\ ...\", @"C:\ ...\")
            InlineData(@"C:\. \", @"C:\. \")
            InlineData(@"C:\.. \", @"C:\.. \")
            InlineData(@"C:\... \", @"C:\... \")
            InlineData(@"C:\A \", @"C:\A \")
            InlineData(@"C:\A \B", @"C:\A \B")

            // Same as above with prefix
            InlineData(@"\\?\C:\", @"\\?\C:\")
            InlineData(@"\\?\C:\ ", @"\\?\C:\")
            InlineData(@"\\?\C:\.", @"\\?\C:")           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\..", @"\\?\")            // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...", @"\\?\C:\")
            InlineData(@"\\?\C:\ .", @"\\?\C:\")
            InlineData(@"\\?\C:\ ..", @"\\?\C:\")
            InlineData(@"\\?\C:\ ...", @"\\?\C:\")
            InlineData(@"\\?\C:\. ", @"\\?\C:\")
            InlineData(@"\\?\C:\.. ", @"\\?\C:\")
            InlineData(@"\\?\C:\... ", @"\\?\C:\")
            InlineData(@"\\?\C:\.\", @"\\?\C:\")
            InlineData(@"\\?\C:\..\", @"\\?\")           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...\", @"\\?\C:\...\")

            // How deep can we go with prefix
            InlineData(@"\\?\C:\..\..", @"\\?\")
            InlineData(@"\\?\C:\..\..\..", @"\\?\")

            // Pipe tests
            InlineData(@"\\.\pipe", @"\\.\pipe")
            InlineData(@"\\.\pipe\", @"\\.\pipe\")
            InlineData(@"\\?\pipe", @"\\?\pipe")
            InlineData(@"\\?\pipe\", @"\\?\pipe\")

            // Basic dot space handling with UNCs
            InlineData(@"\\Server\Share\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.", @"\\Server\Share")                      // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\..", @"\\Server\Share")                     // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\...", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ .", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ..", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ...", @"\\Server\Share\")
            InlineData(@"\\Server\Share\. ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.. ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\... ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\..\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\...\", @"\\Server\Share\...\")

            // Slash direction makes no difference
            InlineData(@"//Server\Share\", @"\\Server\Share\")
            InlineData(@"//Server\Share\ ", @"\\Server\Share\")
            InlineData(@"//Server\Share\.", @"\\Server\Share")                      // UNCs can eat trailing separator
            InlineData(@"//Server\Share\..", @"\\Server\Share")                     // UNCs can eat trailing separator
            InlineData(@"//Server\Share\...", @"\\Server\Share\")
            InlineData(@"//Server\Share\ .", @"\\Server\Share\")
            InlineData(@"//Server\Share\ ..", @"\\Server\Share\")
            InlineData(@"//Server\Share\ ...", @"\\Server\Share\")
            InlineData(@"//Server\Share\. ", @"\\Server\Share\")
            InlineData(@"//Server\Share\.. ", @"\\Server\Share\")
            InlineData(@"//Server\Share\... ", @"\\Server\Share\")
            InlineData(@"//Server\Share\.\", @"\\Server\Share\")
            InlineData(@"//Server\Share\..\", @"\\Server\Share\")
            InlineData(@"//Server\Share\...\", @"\\Server\Share\...\")

            // Slash count breaks rooting
            InlineData(@"\\\Server\Share\", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\.", @"\\\Server\Share")                     // UNCs can eat trailing separator
            InlineData(@"\\\Server\Share\..", @"\\\Server")                          // Paths without 2 initial slashes will not root the share
            InlineData(@"\\\Server\Share\...", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ .", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ ..", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ ...", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\. ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\.. ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\... ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\.\", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\..\", @"\\\Server\")                       // Paths without 2 initial slashes will not root the share
            InlineData(@"\\\Server\Share\...\", @"\\\Server\Share\...\")

            // Inital slash count is always kept
            InlineData(@"\\\\Server\Share\", @"\\\Server\Share\")
            InlineData(@"\\\\\Server\Share\", @"\\\Server\Share\")

            // Extended paths root to \\?\
            InlineData(@"\\?\UNC\Server\Share\", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.", @"\\?\UNC\Server\Share")
            InlineData(@"\\?\UNC\Server\Share\..", @"\\?\UNC\Server")               // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ .", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ..", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ...", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\. ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.. ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\... ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.\", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\..\", @"\\?\UNC\Server\")             // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...\", @"\\?\UNC\Server\Share\...\")

            // How deep can we go with prefix
            InlineData(@"\\?\UNC\Server\Share\..\..", @"\\?\UNC")
            InlineData(@"\\?\UNC\Server\Share\..\..\..", @"\\?\")
            InlineData(@"\\?\UNC\Server\Share\..\..\..\..", @"\\?\")

            // Root slash behavior
            InlineData(@"C:/", @"C:\")
            InlineData(@"C:/..", @"C:\")
            InlineData(@"//Server/Share", @"\\Server\Share")
            InlineData(@"//Server/Share/..", @"\\Server\Share")
            InlineData(@"//Server//Share", @"\\Server\Share")
            InlineData(@"//Server//Share/..", @"\\Server\")                         // Double slash shares normalize but don't root correctly
            InlineData(@"//Server\\Share/..", @"\\Server\")
            InlineData(@"//?/", @"\\?\")

            // Device behavior
            InlineData(@"CON", @"\\.\CON")
            InlineData(@"CON:Alt", @"\\.\CON")
            InlineData(@"LPT9", @"\\.\LPT9")

            InlineData(@"C:\A\B\.\..\C", @"C:\A\C")
            ]
        public void ValidateKnownFixedBehaviors(string value, string expected)
        {
            NativeMethods.FileManagement.GetFullPathName(value).Should().Be(expected, $"source was {value}");
        }

        [Theory
            InlineData(@"C:\PROGRA~1", @"C:\Program Files")
            InlineData(@"C:\.\PROGRA~1", @"C:\.\Program Files")
            ]
        public void ValidateLongPathNameBehaviors(string value, string expected)
        {
            using (new TempCurrentDirectory(@"C:\Users"))
            {
                NativeMethods.FileManagement.GetLongPathName(value).Should().Be(expected);
            }
        }

        [Fact]
        public void LongPathNameThrowsFileNotFound()
        {
            string path = Path.GetRandomFileName();
            Action action = () => NativeMethods.FileManagement.GetLongPathName(path);
            action.ShouldThrow<FileNotFoundException>();
        }

        [Fact]
        public void FinalPathNameLongPathPrefixRoundTripBehavior()
        {
            using (var cleaner = new TestFileCleaner(useDotNet: false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                string filePath = Paths.Combine(longPath, Path.GetRandomFileName());

                cleaner.FileService.CreateDirectory(longPath);
                cleaner.FileService.WriteAllText(filePath, "FinalPathNameLongPathPrefixRoundTripBehavior");

                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();

                    string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);

                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"\\?\Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }

        [Fact]
        public void FinalPathNameBehavior()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("FinalPathNameBehavior");

                using (var handle = NativeMethods.FileManagement.CreateFile(filePath.ToLower(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();

                    string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"\\?\Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }

        [Fact]
        public void FinalPathNameVolumeNameBehavior()
        {
            // This test is asserting that the original volume name has nothing to do with the volume GetFinalPathNameByHandle returns
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("FinalPathNameVolumeNameBehavior");

                string canonicalRoot = cleaner.ExtendedFileService.GetCanonicalRoot(cleaner.FileService, filePath);
                string replacedPath = Paths.ReplaceCasing(filePath, canonicalRoot);

                using (var handle = NativeMethods.FileManagement.CreateFile(replacedPath.ToLower(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();

                    string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"\\?\Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }

        [Fact]
        public void FinalPathNameLinkBehavior()
        {
            var extendedFileService = new ExtendedFileService();
            if (!extendedFileService.CanCreateSymbolicLinks()) return;

            // GetFinalPathName always points to the linked file unless you specifically open the reparse point
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, "Target");
                string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);

                cleaner.FileService.WriteAllText(filePath, "CreateSymbolicLinkToFile");

                string symbolicLink = Paths.Combine(cleaner.TempFolder, "Link");
                string extendedLink = Paths.AddExtendedPrefix(symbolicLink, addIfUnderLegacyMaxPath: true);
                NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);
                NativeMethods.FileManagement.FileExists(symbolicLink).Should().BeTrue("symbolic link should exist");

                // GetFinalPathName should normalize the casing, pushing ToUpper to validate
                using (var handle = NativeMethods.FileManagement.CreateFile(symbolicLink.ToUpperInvariant(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                }

                using (var handle = NativeMethods.FileManagement.CreateFile(symbolicLink.ToUpperInvariant(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, NativeMethods.FileManagement.AllFileAttributeFlags.FILE_FLAG_OPEN_REPARSE_POINT))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedLink);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedLink);
                }
            }
        }
    }
}
