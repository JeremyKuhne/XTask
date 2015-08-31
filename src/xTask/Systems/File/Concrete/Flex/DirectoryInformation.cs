// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.Flex
{
    using Interop;
    using System;
    using System.Collections.Generic;

    internal class DirectoryInformation : FileSystemInformation, IDirectoryInformation
    {
        // True if this was populated from attributes (can't find file on volumes, e.g. C:\)
        bool fromAttributes = false;

        private DirectoryInformation(IFileService fileService)
            : base(fileService)
        {
        }

        new static internal IFileSystemInformation Create(NativeMethods.FileManagement.FindResult findResult, IFileService fileService)
        {
            if (!findResult.Attributes.HasFlag(System.IO.FileAttributes.Directory)) throw new ArgumentOutOfRangeException(nameof(findResult));

            var info = new DirectoryInformation(fileService);
            info.PopulateData(findResult);
            return info;
        }

        new static internal IFileSystemInformation Create(string path, System.IO.FileAttributes attributes, IFileService fileService)
        {
            if (!attributes.HasFlag(System.IO.FileAttributes.Directory)) throw new ArgumentOutOfRangeException(nameof(attributes));

            var info = new DirectoryInformation(fileService);
            info.PopulateData(path, attributes);
            info.fromAttributes = true;

            return info;
        }

        protected override void Refresh(bool fromAttributes)
        {
            base.Refresh(this.fromAttributes);
        }

        protected override void PopulateData(NativeMethods.FileManagement.FindResult findResult)
        {
            base.PopulateData(findResult);
        }

        public IEnumerable<IFileSystemInformation> EnumerateChildren(
            ChildType childType = ChildType.File,
            string searchPattern = "*",
            System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly,
            System.IO.FileAttributes excludeAttributes = System.IO.FileAttributes.Hidden | System.IO.FileAttributes.System | System.IO.FileAttributes.ReparsePoint)
        {
            return EnumerateChildrenInternal(this.Path, childType, searchPattern, searchOption, excludeAttributes, this.FileService);
        }

        internal static IEnumerable<IFileSystemInformation> EnumerateChildrenInternal(
            string directory,
            ChildType childType,
            string searchPattern,
            System.IO.SearchOption searchOption,
            System.IO.FileAttributes excludeAttributes,
            IFileService fileService)
        {
            var firstFile = NativeMethods.FileManagement.FindFirstFile(Paths.Combine(directory, searchPattern));
            var findInfo = firstFile;

            if (firstFile != null)
            {
                // Look for specified file/directories
                do
                {
                    bool isDirectory = findInfo.Attributes.HasFlag(System.IO.FileAttributes.Directory);

                    if ((findInfo.Attributes & excludeAttributes) == 0
                        && findInfo.FileName != "."
                        && findInfo.FileName != ".."
                        && ((isDirectory && childType == ChildType.Directory)
                            || (!isDirectory && childType == ChildType.File)))
                    {
                        yield return FileSystemInformation.Create(findInfo, fileService);
                    }

                    findInfo = NativeMethods.FileManagement.FindNextFile(firstFile);
                } while (findInfo != null);

                firstFile.FindHandle.Close();
            }

            if (searchOption != System.IO.SearchOption.AllDirectories) yield break;

            // Need to recurse to find additional matches
            firstFile = NativeMethods.FileManagement.FindFirstFile(Paths.Combine(directory, "*"), directoriesOnly: true);
            if (firstFile == null) yield break;
            findInfo = firstFile;

            do
            {
                if ((findInfo.Attributes & excludeAttributes) == 0
                    && findInfo.Attributes.HasFlag(System.IO.FileAttributes.Directory)
                    && findInfo.FileName != "."
                    && findInfo.FileName != "..")
                {
                    IFileSystemInformation childDirectory = DirectoryInformation.Create(findInfo, fileService);
                    foreach (var child in EnumerateChildrenInternal(childDirectory.Path, childType, searchPattern, searchOption, excludeAttributes, fileService))
                    {
                        yield return child;
                    }
                }
                findInfo = NativeMethods.FileManagement.FindNextFile(firstFile);
            } while (findInfo != null);

            firstFile.FindHandle.Close();
        }
    }
}
