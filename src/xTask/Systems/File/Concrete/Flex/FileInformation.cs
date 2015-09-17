// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.Flex
{
    using Interop;
    using Microsoft.Win32.SafeHandles;
    using System;

    internal class FileInformation : FileSystemInformation, IFileInformation
    {
        private byte[] md5Hash;
        private IDirectoryInformation directoryInformation;
        private string directory;

        private FileInformation(IFileService fileService)
            : base (fileService)
        {
        }

        new static internal IFileSystemInformation Create(NativeMethods.FileManagement.FindResult findResult, IFileService fileService)
        {
            if ((findResult.Attributes & System.IO.FileAttributes.Directory) != 0) throw new ArgumentOutOfRangeException(nameof(findResult));

            var fileInfo = new FileInformation(fileService);
            fileInfo.PopulateData(findResult);
            return fileInfo;
        }

        new internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, NativeMethods.FileManagement.BY_HANDLE_FILE_INFORMATION info, IFileService fileService)
        {
            if ((info.dwFileAttributes & System.IO.FileAttributes.Directory) != 0) throw new ArgumentOutOfRangeException(nameof(info));

            var fileInfo = new FileInformation(fileService);
            fileInfo.PopulateData(originalPath, fileHandle, info);
            return fileInfo;
        }

        protected override void PopulateData(NativeMethods.FileManagement.FindResult findResult)
        {
            base.PopulateData(findResult);
            this.Length = findResult.Length;
            this.directory = findResult.BasePath;
        }

        protected override void PopulateData(string originalPath, SafeFileHandle fileHandle, NativeMethods.FileManagement.BY_HANDLE_FILE_INFORMATION info)
        {
            base.PopulateData(originalPath, fileHandle, info);
            this.Length = NativeMethods.HighLowToLong(info.nFileSizeHigh, info.nFileSizeLow);
            this.directory = Paths.GetDirectory(this.Path);
        }

        public ulong Length { get; private set; }

        public IDirectoryInformation Directory
        {
            get
            {
                if (this.directoryInformation == null)
                {
                    this.directoryInformation = (IDirectoryInformation)FileSystemInformation.Create(this.directory, this.FileService);
                }
                return this.directoryInformation;
            }
        }

        public byte[] MD5Hash
        {
            get
            {
                if (this.md5Hash == null)
                {
                    this.md5Hash = this.FileService.GetHash(this.Path);
                }
                return this.md5Hash;
            }
        }

        public override void Refresh()
        {
            this.md5Hash = null;
            this.directoryInformation = null;
            base.Refresh();
        }
    }
}
