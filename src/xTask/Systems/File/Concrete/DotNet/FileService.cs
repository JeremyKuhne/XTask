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
    /// File service that exclusively uses the .NET IO implementation to back it
    /// </summary>
    /// <remarks>
    /// Does not support long paths or extended syntax. (As of .NET 4.6)
    /// 
    /// Standard .NET implementations are not very performant- they typically do way more validation than is necessary
    /// before calling Win32 APIs.
    /// </remarks>
    public class FileService : ExtendedFileService, IExtendedFileService
    {
        public string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
            set { Environment.CurrentDirectory = value; }
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public Stream CreateFileStream(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite)
        {
            return new FileStream(path, mode, access, share);
        }

        public void DeleteDirectory(string path, bool deleteChildren = false)
        {
            Directory.Delete(path, recursive: deleteChildren);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public IFileSystemInformation GetPathInfo(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                return new DirectoryInformation(new DirectoryInfo(path), this);
            }
            else
            {
                return new FileInformation(new FileInfo(path), this);
            }
        }

        public string GetFullPath(string path, string basePath = null)
        {
            if (basePath == null || !Paths.IsRelative(path))
            {
                // Fixed, or we don't have a base path
                return Path.GetFullPath(path);
            }
            else
            {
                return Path.GetFullPath(Paths.Combine(basePath, path));
            }
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public void SetAttributes(string path, FileAttributes attributes)
        {
            File.SetAttributes(path, attributes);
        }

        public void CopyFile(string existingPath, string newPath, bool overwrite = false)
        {
            File.Copy(existingPath, newPath, overwrite);
        }
    }
}
