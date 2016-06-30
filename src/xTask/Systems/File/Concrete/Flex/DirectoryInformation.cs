// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using WInterop.FileManagement;
using WInterop.FileManagement.DataTypes;
using WInterop.Handles.DataTypes;

namespace XTask.Systems.File.Concrete.Flex
{
    internal class DirectoryInformation : FileSystemInformation, IDirectoryInformation
    {
        private DirectoryInformation(IFileService fileService)
            : base(fileService)
        {
        }

        new static internal IFileSystemInformation Create(FindResult findResult, string directory, IFileService fileService)
        {
            if ((findResult.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == 0) throw new ArgumentOutOfRangeException(nameof(findResult));

            var directoryInfo = new DirectoryInformation(fileService);
            directoryInfo.PopulateData(findResult, directory);
            return directoryInfo;
        }

        new static internal IFileSystemInformation Create(string path, System.IO.FileAttributes attributes, IFileService fileService)
        {
            if ((attributes & System.IO.FileAttributes.Directory) == 0) throw new ArgumentOutOfRangeException(nameof(attributes));

            var directoryInfo = new DirectoryInformation(fileService);
            directoryInfo.PopulateData(path, attributes);
            return directoryInfo;
        }

        new internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, FileBasicInfo info, IFileService fileService)
        {
            if ((info.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == 0) throw new ArgumentOutOfRangeException(nameof(info));

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

            // The assertion here is that we want to find files that match the desired pattern in all subdirectories, even if the
            // subdirectories themselves don't match the pattern. That requires two passes to avoid overallocating for directories
            // with a large number of files.

            // First look for items that match the given search pattern in the current directory
            using (FindOperation findOperation = new FindOperation(Paths.Combine(extendedDirectory, searchPattern)))
            {
                FindResult findResult;
                while ((findResult = findOperation.GetNextResult()) != null)
                {
                    bool isDirectory = (findResult.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == FileAttributes.FILE_ATTRIBUTE_DIRECTORY;

                    if ((findResult.Attributes & excludeAttributes) == 0
                        && findResult.FileName != "."
                        && findResult.FileName != ".."
                        && ((isDirectory && childType == ChildType.Directory)
                            || (!isDirectory && childType == ChildType.File)))
                    {
                        yield return FileSystemInformation.Create(findResult, directory, fileService);
                    }
                }
            }

            if (searchOption != System.IO.SearchOption.AllDirectories) yield break;

            // Now recurse into each subdirectory
            using (FindOperation findOperation = new FindOperation(Paths.Combine(extendedDirectory, "*"), directoriesOnly: true))
            {
                FindResult findResult;
                while ((findResult = findOperation.GetNextResult()) != null)
                {
                    // Unfortunately there is no guarantee that the API will return only directories even if we ask for it
                    bool isDirectory = (findResult.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == FileAttributes.FILE_ATTRIBUTE_DIRECTORY;

                    if ((findResult.Attributes & excludeAttributes) == 0
                        && isDirectory
                        && findResult.FileName != "."
                        && findResult.FileName != "..")
                    {
                        foreach (var child in EnumerateChildrenInternal(Paths.Combine(directory, findResult.FileName), childType, searchPattern,
                            searchOption, excludeAttributes, fileService))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }
    }
}
