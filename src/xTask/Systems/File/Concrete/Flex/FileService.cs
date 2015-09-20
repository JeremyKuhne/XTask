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

    /// <summary>
    /// File service that provides fast and accurate file system access. Supports extended syntax and implicit long paths.
    /// </summary>
    /// <remarks>
    /// Also supports UNCs, devices, and alternate data streams.
    /// </remarks>
    internal class FileService : ExtendedFileService, IExtendedFileService
    {
        private CurrentDirectory directory;

        public FileService() : base()
        {
            this.directory = new Flex.CurrentDirectory(this);
        }

        public string CurrentDirectory
        {
            get
            {
                return this.directory.GetCurrentDirectory();
            }
            set
            {
                this.directory.SetCurrentDirectory(value);
            }
        }

        public System.IO.Stream CreateFileStream(string path, System.IO.FileMode mode = System.IO.FileMode.Open, System.IO.FileAccess access = System.IO.FileAccess.Read, System.IO.FileShare share = System.IO.FileShare.ReadWrite)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Flags match what FileStream does internally
            NativeMethods.FileManagement.AllFileAttributeFlags flags = NativeMethods.FileManagement.AllFileAttributeFlags.SECURITY_SQOS_PRESENT | NativeMethods.FileManagement.AllFileAttributeFlags.SECURITY_ANONYMOUS;
            SafeFileHandle handle = NativeMethods.FileManagement.CreateFile(path, access, share, mode, flags);
            System.IO.Stream stream = new System.IO.FileStream(handle, access);
            if (mode == System.IO.FileMode.Append)
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
            }
            return stream;
        }

        public void CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            path = this.GetFullPath(path);
            int pathRootLength = Paths.GetRootLength(path);
            if (pathRootLength < 0) throw NativeMethods.GetIoExceptionForError(NativeMethods.WinError.ERROR_BAD_PATHNAME);

            int i = pathRootLength;
            string subDirectory;
            while (i > 0 && i < path.Length)
            {
                i = path.IndexOf(Paths.DirectorySeparator, i);
                if (i == -1)
                {
                    subDirectory = path;
                    i = path.Length;
                }
                else
                {
                    subDirectory = path.Substring(0, i);
                    i++;
                }

                System.IO.FileAttributes attributes;
                if (!NativeMethods.FileManagement.TryGetFileAttributes(subDirectory, out attributes))
                {
                    // Doesn't exist, try to create it
                    NativeMethods.DirectoryManagement.CreateDirectory(subDirectory);
                }
                else if (attributes.HasFlag(System.IO.FileAttributes.Directory))
                {
                    // Directory exists, move on
                    continue;
                }
                else
                {
                    // File exists
                    throw NativeMethods.GetIoExceptionForError(NativeMethods.WinError.ERROR_FILE_EXISTS, subDirectory);
                }
            }
        }

        public void DeleteFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            NativeMethods.FileManagement.DeleteFile(path);
        }

        private void DeleteDirectoryRecursive(string path)
        {
            System.IO.FileAttributes attributes;
            if (!NativeMethods.FileManagement.TryGetFileAttributes(path, out attributes))
            {
                // Nothing found
                throw NativeMethods.GetIoExceptionForError(NativeMethods.WinError.ERROR_PATH_NOT_FOUND, path);
            }

            if (!attributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                // Not a directory, a file
                throw NativeMethods.GetIoExceptionForError(NativeMethods.WinError.ERROR_FILE_EXISTS, path);
            }

            //if (attributes.HasFlag(FileAttributes.ReadOnly))
            //{
            //    // Make it writable
            //    NativeMethods.FileManagement.SetFileAttributes(path, attributes & ~FileAttributes.ReadOnly);
            //}

            if (!attributes.HasFlag(System.IO.FileAttributes.ReparsePoint))
            {
                // Remove the subdirectories and files
                // Reparse points are simply disconnected, they don't need to be emptied.
                foreach (var file in DirectoryInformation.EnumerateChildrenInternal(
                    directory: path,
                    childType: ChildType.File,
                    searchPattern: "*",
                    searchOption: System.IO.SearchOption.TopDirectoryOnly,
                    excludeAttributes: 0,
                    fileService: this))
                {
                    this.DeleteFile(file.Path);
                }

                foreach (var directory in DirectoryInformation.EnumerateChildrenInternal(
                    directory: path,
                    childType: ChildType.Directory,
                    searchPattern: "*",
                    searchOption: System.IO.SearchOption.TopDirectoryOnly,
                    excludeAttributes: 0,
                    fileService: this))
                {
                    this.DeleteDirectoryRecursive(directory.Path);
                }
            }

            // We've either emptied or we're a reparse point, delete the directory
            NativeMethods.DirectoryManagement.RemoveDirectory(path);
        }

        public void DeleteDirectory(string path, bool deleteChildren = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            path = NativeMethods.FileManagement.GetFullPathName(path);

            if (deleteChildren)
            {
                this.DeleteDirectoryRecursive(path);
            }
            else
            {
                NativeMethods.DirectoryManagement.RemoveDirectory(path);
            }
        }

        public string GetFullPath(string path, string basePath = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (basePath == null || !Paths.IsRelative(path))
            {
                // Fixed, or we don't have a base path
                return NativeMethods.FileManagement.GetFullPathName(path);
            }
            else
            {
                return NativeMethods.FileManagement.GetFullPathName(Paths.Combine(basePath, path));
            }
        }

        public bool IsReadOnly(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            System.IO.FileAttributes attributes;
            if (NativeMethods.FileManagement.TryGetFileAttributes(path, out attributes))
            {
                return attributes.HasFlag(System.IO.FileAttributes.ReadOnly);
            }
            return false;
        }

        public void MakeWritable(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            System.IO.FileAttributes attributes;
            if (NativeMethods.FileManagement.TryGetFileAttributes(path, out attributes))
            {
                if (attributes.HasFlag(System.IO.FileAttributes.ReadOnly))
                {
                    NativeMethods.FileManagement.TrySetFileAttributes(path, attributes & ~System.IO.FileAttributes.ReadOnly);
                }
            }
        }

        public IFileSystemInformation GetPathInfo(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return FileSystemInformation.Create(path, this);
        }

        public System.IO.FileAttributes GetAttributes(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return NativeMethods.FileManagement.GetFileAttributes(path);
        }

        public void SetAttributes(string path, System.IO.FileAttributes attributes)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            NativeMethods.FileManagement.SetFileAttributes(path, attributes);
        }

        public void CopyFile(string existingPath, string newPath, bool overwrite = false)
        {
            if (existingPath == null) throw new ArgumentNullException(nameof(existingPath));
            if (newPath == null) throw new ArgumentNullException(nameof(newPath));

            NativeMethods.FileManagement.CopyFile(existingPath, newPath, overwrite);
        }
    }
}
