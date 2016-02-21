// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Support
{
    using NSubstitute;
    using System;
    using System.IO;
    using System.Text;
    using XTask.Systems.File;

    public static class TestFileServices
    {
        public static IFileService CreateSubstituteForFile(out string path, string content)
        {
            // Using a net share syntax prevents FullPath from resolving against the current working directory
            path = @"\\TestFileService\Tests\" + Path.GetRandomFileName();
            IFileService fileService = Substitute.ForPartsOf<SimpleFileService>(path, content);
            return fileService;
        }
 
        public abstract class SimpleFileService : IFileService
        {
            private string _path;
            private Stream _stream;

            public SimpleFileService(string path, string content)
            {
                _path = path;
                _stream = new MemoryStream(Encoding.UTF8.GetBytes(content ?? ""));
            }

            public Stream CreateFileStream(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite)
            {
                if (string.Equals(path, _path, StringComparison.OrdinalIgnoreCase))
                {
                    return _stream;
                }
                else
                {
                    switch (mode)
                    {
                        case FileMode.Create:
                        case FileMode.CreateNew:
                        case FileMode.OpenOrCreate:
                            return null;
                        default:
                            throw new FileNotFoundException("Test file does not exist.", path);
                    }
                }
            }

            public FileAttributes GetAttributes(string path)
            {
                if (string.Equals(path, this._path, StringComparison.OrdinalIgnoreCase))
                {
                    return FileAttributes.Normal;
                }

                throw new IOException();
            }

            public string GetFullPath(string path, string basePath = null)
            {
                return path;
            }

            public abstract IFileSystemInformation GetPathInfo(string path);
            public abstract void SetAttributes(string path, FileAttributes attributes);
            public abstract string CurrentDirectory { get; set; }
            public abstract void CopyFile(string existingPath, string newPath, bool overwrite = false);
            public abstract void CreateDirectory(string path);
            public abstract void DeleteDirectory(string path, bool deleteChildren = false);
            public abstract void DeleteFile(string path);
        }
    }
}
