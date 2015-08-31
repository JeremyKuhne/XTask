// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.DotNet
{
    using System;
    using System.IO;

    /// <summary>
    /// Simple implementation of a FileInfo wrapper
    /// </summary>
    public class FileInformation : FileSystemInformation, IFileInformation
    {
        private FileInfo fileInfo;
        private byte[] md5Hash;
        private IDirectoryInformation directoryInformation;

        public FileInformation(FileInfo fileInfo, IFileService fileService) : base(fileInfo, fileService)
        {
            this.fileInfo = fileInfo;
        }

        public virtual long Length
        {
            get { return this.fileInfo.Length; }
        }

        public virtual IDirectoryInformation Directory
        {
            get
            {
                if (this.directoryInformation == null)
                {
                    this.directoryInformation = new DirectoryInformation(this.fileInfo.Directory, this.FileService);
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
            base.Refresh();
        }
    }
}
