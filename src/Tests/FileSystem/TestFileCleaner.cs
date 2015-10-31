// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using System.IO;
    using Systems.File;
    using Systems.File.Concrete;
    using Concrete = Systems.File.Concrete;

    public class TestFileCleaner : FileCleaner
    {
        bool useDotNet;
        string originalCurrentDirectory;

        internal static object DirectorySetLock;

        private static IExtendedFileService extendedFileService = new ExtendedFileService();

        static TestFileCleaner()
        {
            DirectorySetLock = new object();
        }

        public TestFileCleaner(bool useDotNet = false)
            : base ("XTaskTests", useDotNet ? (IFileService) new Concrete.DotNet.FileService() : new Concrete.Flex.FileService(extendedFileService))
        {
            this.useDotNet = useDotNet;

            // Try and restore the original current directory if a test fails
            this.originalCurrentDirectory = Directory.GetCurrentDirectory();
        }

        protected override void CleanOrphanedTempFolders()
        {
            // .NET can't handle long paths and we'll be creating a lot of them, so don't let
            // that implementation do this phase of cleanup.
            if (!this.useDotNet)
            {
                base.CleanOrphanedTempFolders();
            }
        }

        protected override bool ThrowOnCleanSelf
        {
            get
            {
                // We want to catch dangling handles, etc.
                return true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            lock (DirectorySetLock)
            {
                if (Directory.GetCurrentDirectory() != originalCurrentDirectory)
                {
                    if (Directory.Exists(originalCurrentDirectory))
                        Directory.SetCurrentDirectory(originalCurrentDirectory);
                }
            }

            base.Dispose(disposing);
        }

        public string GetTestPath(string basePath = null)
        {
            return Paths.Combine(basePath ?? this.TempFolder, Path.GetRandomFileName());
        }

        public string CreateTestFile(string content, string basePath = null)
        {
            string testFile = GetTestPath(basePath);
            FileService.WriteAllText(testFile, content);
            return testFile;
        }

        public string CreateTestDirectory(string basePath = null)
        {
            string testDirectory = GetTestPath(basePath);
            FileService.CreateDirectory(testDirectory);
            return testDirectory;
        }

        public IFileService FileService
        {
            get
            {
                return this.fileServiceProvider;
            }
        }

        public IExtendedFileService ExtendedFileService
        {
            get
            {
                return extendedFileService;
            }
        }
    }
}
