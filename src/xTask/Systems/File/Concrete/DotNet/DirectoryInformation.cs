// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.DotNet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using XTask.Collections;

    public class DirectoryInformation : FileSystemInformation, IDirectoryInformation
    {
        private DirectoryInfo _directoryInfo;

        public DirectoryInformation(DirectoryInfo directoryInfo, IFileService fileService) : base(directoryInfo, fileService)
        {
            _directoryInfo = directoryInfo;
        }

        public IEnumerable<IFileSystemInformation> EnumerateChildren(
            ChildType childType = ChildType.File,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            FileAttributes excludeAttributes = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)
         {
            if (childType == ChildType.Directory)
            {
                return EnumerateDirectories(searchPattern, searchOption, excludeAttributes);
            }
            else
            {
                return EnumerateFiles(_directoryInfo, searchPattern, searchOption, excludeAttributes);
            }
         }

        private IEnumerable<IDirectoryInformation> EnumerateDirectories(string searchPattern, SearchOption searchOption, FileAttributes excludeAttributes)
        {
            foreach (DirectoryInfo info in _directoryInfo.EnumerateDirectories(searchPattern: searchPattern, searchOption: searchOption))
            {
                if ((info.Attributes & excludeAttributes) == 0)
                {
                    yield return new DirectoryInformation(info, FileService);
                }
            }
        }

        private IEnumerable<IFileInformation> EnumerateFiles(DirectoryInfo directory, string searchPattern, SearchOption searchOption, FileAttributes excludeAttributes)
        {
            IEnumerable<IFileInformation> allFiles =
                from file in directory.EnumerateFiles(searchPattern)
                where (file.Attributes & excludeAttributes) == 0
                select new FileInformation(file, FileService);

            if (searchOption == SearchOption.AllDirectories)
            {
                allFiles = allFiles.Concat(
                    from subDirectory in directory.EnumerateDirectories()
                    where (subDirectory.Attributes & excludeAttributes) == 0
                    select EnumerateFiles(subDirectory, searchPattern, searchOption, excludeAttributes));
            }

            return allFiles;
        }
    }
}
