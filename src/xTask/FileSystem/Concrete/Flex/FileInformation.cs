// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem.Concrete.Flex
{
    using Interop;
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
            if (findResult.Attributes.HasFlag(System.IO.FileAttributes.Directory)) throw new ArgumentOutOfRangeException(nameof(findResult));

            var info = new FileInformation(fileService);
            info.PopulateData(findResult);
            return info;
        }

        protected override void PopulateData(NativeMethods.FileManagement.FindResult findResult)
        {
            this.Length = findResult.Length;
            this.directory = findResult.BasePath;
            base.PopulateData(findResult);
        }

        public long Length { get; private set; }

        public IDirectoryInformation Directory
        {
            get
            {
                if (this.directoryInformation == null)
                {
                    var findResult = NativeMethods.FileManagement.FindFirstFile(this.directory);
                    this.directoryInformation = (IDirectoryInformation)DirectoryInformation.Create(findResult, this.FileService);
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

        protected override void Refresh(bool fromAttributes)
        {
            this.md5Hash = null;
            this.directoryInformation = null;
            base.Refresh();
        }
    }
}
