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
        private FileInfo _fileInfo;
        private byte[] _md5Hash;
        private IDirectoryInformation _directoryInformation;

        public FileInformation(FileInfo fileInfo, IFileService fileService) : base(fileInfo, fileService)
        {
            _fileInfo = fileInfo;
        }

        public virtual ulong Length
        {
            get { return (ulong)_fileInfo.Length; }
        }

        public virtual IDirectoryInformation Directory
        {
            get
            {
                if (_directoryInformation == null)
                {
                    _directoryInformation = new DirectoryInformation(_fileInfo.Directory, FileService);
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
            base.Refresh();
        }
    }
}
