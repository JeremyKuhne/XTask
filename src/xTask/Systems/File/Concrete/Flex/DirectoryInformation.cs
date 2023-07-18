// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES;

#if NETFRAMEWORK
using IO = Microsoft.IO;
using Microsoft.IO.Enumeration;
#else
using IO = System.IO;
using System.IO.Enumeration;
#endif

namespace XTask.Systems.File.Concrete.Flex;

internal class DirectoryInformation : FileSystemInformation, IDirectoryInformation
{
    private DirectoryInformation(IFileService fileService)
        : base(fileService)
    {
    }

    new static internal IFileSystemInformation Create(ref FileSystemEntry findData, IFileService fileService)
    {
        if (!findData.Attributes.HasFlag(System.IO.FileAttributes.Directory))
        {
            throw new ArgumentOutOfRangeException(nameof(findData));
        }

        DirectoryInformation directoryInfo = new(fileService);
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

    new internal static IFileSystemInformation Create(
        string originalPath,
        SafeFileHandle fileHandle,
        FILE_BASIC_INFO info,
        IFileService fileService)
    {
        if (!((FILE_FLAGS_AND_ATTRIBUTES)info.FileAttributes).HasFlag(FILE_ATTRIBUTE_DIRECTORY))
        {
            throw new ArgumentOutOfRangeException(nameof(info));
        }

        DirectoryInformation directoryInfo = new(fileService);
        directoryInfo.PopulateData(originalPath, fileHandle, info);
        return directoryInfo;
    }

    public IEnumerable<IFileSystemInformation> EnumerateChildren(
        ChildType childType = ChildType.File,
        string searchPattern = "*",
        System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly,
        System.IO.FileAttributes excludeAttributes = System.IO.FileAttributes.Hidden | System.IO.FileAttributes.System | System.IO.FileAttributes.ReparsePoint)
    {
        return EnumerateChildrenInternal(Path,
            childType, searchPattern, searchOption, excludeAttributes, FileService);
    }

    internal static IEnumerable<IFileSystemInformation> EnumerateChildrenInternal(
        string directory,
        ChildType childType,
        string searchPattern,
        System.IO.SearchOption searchOption,
        System.IO.FileAttributes excludeAttributes,
        IFileService fileService)
    {
        if (childType == ChildType.File)
        {
            excludeAttributes |= System.IO.FileAttributes.Directory;
        }

        return new FileSystemEnumerable<IFileSystemInformation>(
            directory,
            (ref FileSystemEntry entry) => FileSystemInformation.Create(ref entry, fileService),
            new IO.EnumerationOptions()
            {
                RecurseSubdirectories = searchOption == System.IO.SearchOption.AllDirectories,
                AttributesToSkip = excludeAttributes
            })
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry)
                => (childType == ChildType.Directory || !entry.Attributes.HasFlag(System.IO.FileAttributes.Directory))
                    && FileSystemName.MatchesSimpleExpression(searchPattern.AsSpan(), entry.FileName)
        };
    }
}
