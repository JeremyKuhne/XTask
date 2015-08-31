// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using XTask.Systems.File;
    using Concrete = XTask.Systems.File.Concrete;

    public class TestFileCleaner : FileCleaner
    {
        bool useDotNet;

        public TestFileCleaner(bool useDotNet = true)
            : base ("XTaskTests", useDotNet ? (IFileService) new Concrete.DotNet.FileService() : new Concrete.Flex.FileService())
        {
            this.useDotNet = useDotNet;
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
    }
}
