// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using System;
using WInterop.Storage;

namespace XTask.Systems.File.Concrete.Flex
{
    internal class FileInformation : FileSystemInformation, IFileInformation
    {
        private byte[] _md5Hash;
        private IDirectoryInformation _directoryInformation;
        private string _directory;

        private FileInformation(IFileService fileService)
            : base (fileService)
        {
        }

        new static internal IFileSystemInformation Create(ref RawFindData findData, IFileService fileService)
        {
            if ((findData.FileAttributes & FileAttributes.Directory) != 0) throw new ArgumentOutOfRangeException(nameof(findData));

            var fileInfo = new FileInformation(fileService);
            fileInfo.PopulateData(ref findData);
            fileInfo.Length = findData.FileSize;
            return fileInfo;
        }

        new internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, FileBasicInformation info, IFileService fileService)
        {
            if ((info.FileAttributes & FileAttributes.Directory) != 0) throw new ArgumentOutOfRangeException(nameof(info));

            var fileInfo = new FileInformation(fileService);
            fileInfo.PopulateData(originalPath, fileHandle, info);
            return fileInfo;
        }

        protected override void PopulateData(string originalPath, SafeFileHandle fileHandle, FileBasicInformation info)
        {
            base.PopulateData(originalPath, fileHandle, info);
            Length = (ulong)Storage.GetFileSize(fileHandle);
            _directory = Paths.GetDirectory(Path);
        }

        public ulong Length { get; private set; }

        public IDirectoryInformation Directory
        {
            get
            {
                if (_directoryInformation == null)
                {
                    _directoryInformation = (IDirectoryInformation)Create(_directory, FileService);
                }
                return _directoryInformation;
            }
        }

        public byte[] MD5Hash
        {
            get
            {
                if (_md5Hash == null)
                {
                    _md5Hash = FileService.GetHash(Path);
                }
                return _md5Hash;
            }
        }

        public override void Refresh()
        {
            _md5Hash = null;
            _directoryInformation = null;
            base.Refresh();
        }
    }
}
