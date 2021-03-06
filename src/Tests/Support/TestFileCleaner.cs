﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Support
{
    using System.IO;
    using Systems.File;
    using Systems.File.Concrete;
    using Concrete = Systems.File.Concrete;

    public class TestFileCleaner : FileCleaner
    {
        bool _useDotNet;

        private static IExtendedFileService _extendedFileService = new ExtendedFileService();

        public TestFileCleaner(bool useDotNet = false)
            : base ("XTaskTests", useDotNet ? (IFileService) new Concrete.DotNet.FileService() : new Concrete.Flex.FileService(_extendedFileService))
        {
            _useDotNet = useDotNet;
        }

        protected override void CleanOrphanedTempFolders()
        {
            // .NET can't handle long paths and we'll be creating a lot of them, so don't let
            // that implementation do this phase of cleanup.
            if (!_useDotNet)
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

        public string GetTestPath(string basePath = null)
        {
            return Paths.Combine(basePath ?? TempFolder, Path.GetRandomFileName());
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
                return _fileServiceProvider;
            }
        }

        public IExtendedFileService ExtendedFileService
        {
            get
            {
                return _extendedFileService;
            }
        }
    }
}
