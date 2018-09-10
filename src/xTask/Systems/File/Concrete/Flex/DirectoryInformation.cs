// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using WInterop.Storage;

namespace XTask.Systems.File.Concrete.Flex
{
    internal class DirectoryInformation : FileSystemInformation, IDirectoryInformation
    {
        private DirectoryInformation(IFileService fileService)
            : base(fileService)
        {
        }

        new static internal IFileSystemInformation Create(ref RawFindData findData, IFileService fileService)
        {
            if ((findData.FileAttributes & FileAttributes.Directory) == 0) throw new ArgumentOutOfRangeException(nameof(findData));

            var directoryInfo = new DirectoryInformation(fileService);
            directoryInfo.PopulateData(ref findData);
            return directoryInfo;
        }

        new static internal IFileSystemInformation Create(string path, System.IO.FileAttributes attributes, IFileService fileService)
        {
            if ((attributes & System.IO.FileAttributes.Directory) == 0) throw new ArgumentOutOfRangeException(nameof(attributes));

            var directoryInfo = new DirectoryInformation(fileService);
            directoryInfo.PopulateData(path, attributes);
            return directoryInfo;
        }

        new internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, FileBasicInformation info, IFileService fileService)
        {
            if ((info.FileAttributes & FileAttributes.Directory) == 0) throw new ArgumentOutOfRangeException(nameof(info));

            var directoryInfo = new DirectoryInformation(fileService);
            directoryInfo.PopulateData(originalPath, fileHandle, info);
            return directoryInfo;
        }

        protected override void PopulateData(FindResult findResult, string directory)
        {
            base.PopulateData(findResult, directory);
        }

        public IEnumerable<IFileSystemInformation> EnumerateChildren(
            ChildType childType = ChildType.File,
            string searchPattern = "*",
            System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly,
            System.IO.FileAttributes excludeAttributes = System.IO.FileAttributes.Hidden | System.IO.FileAttributes.System | System.IO.FileAttributes.ReparsePoint)
        {
            return EnumerateChildrenInternal(Path,
                childType, searchPattern, searchOption, unchecked((FileAttributes)excludeAttributes), FileService);
        }

        internal static IEnumerable<IFileSystemInformation> EnumerateChildrenInternal(
            string directory,
            ChildType childType,
            string searchPattern,
            System.IO.SearchOption searchOption,
            FileAttributes excludeAttributes,
            IFileService fileService)
        {
            // We want to be able to see all files as we recurse and open new find handles (that might be over MAX_PATH).
            // We've already normalized our base directory.
            string extendedDirectory = Paths.AddExtendedPrefix(directory);

            var transformFilter = new FindTransformFilter(excludeAttributes, fileService);
            FindOperation<IFileSystemInformation> findOperation = new FindOperation<IFileSystemInformation>(
                extendedDirectory,
                searchPattern,
                recursive: searchOption == System.IO.SearchOption.AllDirectories ? true : false,
                transformFilter,
                transformFilter);

            return findOperation;
        }
    }
}
