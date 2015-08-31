// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.Flex
{
    using Interop;
    using System;

    internal class FileSystemInformation : IFileSystemInformation
    {
        protected IFileService FileService { get; private set; }

        protected FileSystemInformation(IFileService fileService)
        {
            this.FileService = fileService;
        }

        protected virtual void PopulateData(NativeMethods.FileManagement.FindResult findResult)
        {
            this.Path = Paths.Combine(findResult.BasePath, findResult.FileName);
            this.Attributes = findResult.Attributes;
            this.CreationTime = findResult.Creation;
            this.LastAccessTime = findResult.LastAccess;
            this.LastWriteTime = findResult.LastWrite;
            this.Name = findResult.FileName;
            this.Exists = true;
        }

        protected void PopulateData(string path, System.IO.FileAttributes attributes)
        {
            this.Path = path;
            this.Name = path;
            this.Attributes = attributes;
            this.Exists = true;
        }

        internal static IFileSystemInformation Create(NativeMethods.FileManagement.FindResult findResult, IFileService fileService)
        {
            if (findResult.Attributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                return DirectoryInformation.Create(findResult, fileService);
            }
            else
            {
                return FileInformation.Create(findResult, fileService);
            }
        }

        internal static IFileSystemInformation Create(string path, System.IO.FileAttributes attributes, IFileService fileService)
        {
            if (attributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                return DirectoryInformation.Create(path, attributes, fileService);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        internal static IFileSystemInformation Create(string path, IFileService fileService)
        {
            path = fileService.GetFullPath(path);

            try
            {
                var findResult = NativeMethods.FileManagement.FindFirstFile(path, directoriesOnly: false, getAlternateName: false, returnNullIfNotFound: false);
                var info = Create(findResult, fileService);
                findResult.FindHandle.Close();
                return info;
            }
            catch (System.IO.IOException)
            {
                // Could be a root directory (e.g. C:), can't do FindFile
                if (Paths.IsPathRelative(path))
                {
                    throw;
                }

                System.IO.FileAttributes attributes = NativeMethods.FileManagement.GetFileAttributes(path);
                return Create(path, attributes, fileService);
            }
        }

        public System.IO.FileAttributes Attributes { get; private set; }

        public DateTime CreationTime { get; private set; }

        public DateTime LastAccessTime { get; private set; }

        public DateTime LastWriteTime { get; private set; }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public bool Exists { get; private set; }

        public void Refresh()
        {
            this.Refresh(false);
        }

        protected virtual void Refresh(bool fromAttributes = false)
        {
            try
            {
                if (fromAttributes)
                {
                    System.IO.FileAttributes attributes = NativeMethods.FileManagement.GetFileAttributes(this.Path);
                    this.PopulateData(this.Path, attributes);
                }
                else
                {
                    var findResult = NativeMethods.FileManagement.FindFirstFile(this.Path);
                    this.PopulateData(findResult);
                    findResult.FindHandle.Close();
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                this.Exists = false;
            }
        }
    }
}
