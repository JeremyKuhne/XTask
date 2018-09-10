// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using WInterop.Storage;

namespace XTask.Systems.File.Concrete.Flex
{
    public class FindTransformFilter : IFindTransform<IFileSystemInformation>, IFindFilter
    {
        private FileAttributes _excludeAttributes;
        private IFileService _fileService;

        public FindTransformFilter(FileAttributes excludeAttributes, IFileService fileService)
        {
            _excludeAttributes = excludeAttributes;
            _fileService = fileService;
        }

        public bool Match(ref RawFindData findData)
        {
            return (findData.FileAttributes & _excludeAttributes) == 0
                && !findData.FileName.SequenceEqual("..".AsSpan())
                && !findData.FileName.SequenceEqual(".".AsSpan());
        }

        public IFileSystemInformation TransformResult(ref RawFindData findData)
        {
            return FileSystemInformation.Create(ref findData, _fileService);
        }
    }
}
