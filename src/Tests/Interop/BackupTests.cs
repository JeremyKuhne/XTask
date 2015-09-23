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
    using System.Linq;
    using XTask.Systems.File;
    using XTask.Interop;
    using Xunit;

    public class BackupTests
    {
        [Fact]
        public void NoAlternateStreamData()
        {
            using (var cleaner = new TestFileCleaner())
            {
                NativeMethods.Backup.GetAlternateStreamInformation(cleaner.CreateTestFile("NoAlternateStreamData")).Should().BeEmpty();
            }
        }

        [Fact]
        public void OneAlternateDataStream()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string testFile = cleaner.CreateTestFile("OneAlternateDataStream");
                string firstStream = testFile + ":First";
                cleaner.FileService.WriteAllText(firstStream, "First alternate data stream");

                var info = NativeMethods.Backup.GetAlternateStreamInformation(testFile);
                info.Should().HaveCount(1);
                info.First().Name.Should().Be(":First:$DATA");
            }
        }
    }
}
