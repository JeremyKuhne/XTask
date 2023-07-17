// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace XTask.Systems.File
{
    /// <summary>
    ///  Information about a directory.
    /// </summary>
    public interface IDirectoryInformation : IFileSystemInformation
    {
        /// <summary>
        ///  Walk files of the specified type in the current directory. Allows filtering by attributes.
        ///  Folders with the given attributes will be skipped as well.
        /// </summary>
        IEnumerable<IFileSystemInformation> EnumerateChildren(
            ChildType childType = ChildType.File,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            FileAttributes excludeAttributes = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint);
    }
}
