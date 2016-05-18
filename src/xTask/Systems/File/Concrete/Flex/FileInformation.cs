// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using WInterop.FileManagement;
using WInterop.Handles;

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

        new static internal IFileSystemInformation Create(FindResult findResult, string directory, IFileService fileService)
        {
            if ((findResult.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != 0) throw new ArgumentOutOfRangeException(nameof(findResult));

            var fileInfo = new FileInformation(fileService);
            fileInfo.PopulateData(findResult, directory);
            return fileInfo;
        }

        new internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, FileBasicInfo info, IFileService fileService)
        {
            if ((info.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != 0) throw new ArgumentOutOfRangeException(nameof(info));

            var fileInfo = new FileInformation(fileService);
            fileInfo.PopulateData(originalPath, fileHandle, info);
            return fileInfo;
        }

        protected override void PopulateData(FindResult findResult, string directory)
        {
            base.PopulateData(findResult, directory);
            Length = findResult.Length;
            _directory = findResult.OriginalPath;
        }

        protected override void PopulateData(string originalPath, SafeFileHandle fileHandle, FileBasicInfo info)
        {
            base.PopulateData(originalPath, fileHandle, info);
            Length = (ulong)FileMethods.GetFileSize(fileHandle);
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
