// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class FileInformationExtensions
    {
        /// <summary>
        /// Walk files in the current directory. Allows filtering by attributes. Folders with the given attributes will be skipped as well.
        /// </summary>
        /// <param name="fileSystemInformation">If the information is not a directory, returns an empty enumerable.</param>
        public static IEnumerable<IFileInformation> EnumerateFiles(
            this IFileSystemInformation fileSystemInformation,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            FileAttributes excludeAttributes = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)
        {
            var directoryInfo = fileSystemInformation as IDirectoryInformation;
            if (directoryInfo == null) return Enumerable.Empty<IFileInformation>();
            return directoryInfo.EnumerateChildren(ChildType.File, searchPattern, searchOption, excludeAttributes).OfType<IFileInformation>();
        }

        /// <summary>
        /// Walk directories in the current directory. Allows filtering by attributes. Folders with the given attributes will be skipped as well.
        /// </summary>
        /// <param name="fileSystemInformation">If the information is not a directory, returns an empty enumerable.</param>
        public static IEnumerable<IDirectoryInformation> EnumerateDirectories(
            this IFileSystemInformation fileSystemInformation,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            FileAttributes excludeAttributes = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)
        {
            var directoryInfo = fileSystemInformation as IDirectoryInformation;
            if (directoryInfo == null) return Enumerable.Empty<IDirectoryInformation>();
            return directoryInfo.EnumerateChildren(ChildType.Directory, searchPattern, searchOption, excludeAttributes).OfType<IDirectoryInformation>();
        }

    }
}
