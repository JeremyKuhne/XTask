// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem.Concrete.DotNet
{
    using System;
    using System.IO;

    public abstract class FileSystemInformation : IFileSystemInformation
    {
        private FileSystemInfo fileSystemInfo;
        protected IFileService FileService { get; private set; }

        protected FileSystemInformation(FileSystemInfo fileSystemInfo, IFileService fileService)
        {
            this.FileService = fileService;
            this.fileSystemInfo = fileSystemInfo;
        }

        public virtual string Path
        {
            get { return this.fileSystemInfo.FullName; }
        }

        public virtual string Name
        {
            get { return this.fileSystemInfo.Name; }
        }

        public virtual bool Exists
        {
            get { return this.fileSystemInfo.Exists; }
        }

        public virtual DateTime CreationTime
        {
            get { return this.fileSystemInfo.CreationTime; }
        }

        public virtual DateTime LastAccessTime
        {
            get { return this.fileSystemInfo.LastAccessTime; }
        }

        public virtual DateTime LastWriteTime
        {
            get { return this.fileSystemInfo.LastWriteTime; }
        }

        public virtual FileAttributes Attributes
        {
            get
            {
                return this.fileSystemInfo.Attributes;
            }
            set
            {
                this.fileSystemInfo.Attributes = value;
            }
        }

        public virtual void Refresh()
        {
            this.fileSystemInfo.Refresh();
        }
    }
}
