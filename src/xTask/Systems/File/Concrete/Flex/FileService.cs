// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using WInterop.Errors;
using WInterop.Storage;

namespace XTask.Systems.File.Concrete.Flex
{
    /// <summary>
    /// File service that provides fast and accurate file system access. Supports extended syntax and implicit long paths.
    /// Maintains a separate, enhanced current directory that supports alternate volume names.
    /// </summary>
    /// <remarks>
    /// Also supports UNCs, devices, and alternate data streams.
    /// </remarks>
    internal class FileService : IFileService
    {
        private CurrentDirectory _directory;

        public FileService(IExtendedFileService extendedFileService, string initialCurrentDirectory = null)
        {
            _directory = new CurrentDirectory(this, extendedFileService, initialCurrentDirectory);
        }

        public string CurrentDirectory
        {
            get
            {
                return _directory.GetCurrentDirectory();
            }
            set
            {
                _directory.SetCurrentDirectory(value);
            }
        }

        public System.IO.Stream CreateFileStream(string path, System.IO.FileMode mode = System.IO.FileMode.Open, System.IO.FileAccess access = System.IO.FileAccess.Read, System.IO.FileShare share = System.IO.FileShare.ReadWrite)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            path = Paths.AddExtendedPrefix(NormalizeIfNotExtended(path));

            // Flags match what FileStream does internally
            return Storage.CreateFileStream(
                path,
                access,
                share,
                mode,
                fileAttributes: 0,
                fileFlags: FileFlags.None,
                securityFlags: SecurityQosFlags.QosPresent | SecurityQosFlags.Anonymous);
        }

        public void CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            path = GetFullPath(path);
            int pathRootLength = Paths.GetRootLength(path);
            if (pathRootLength < 0) throw WindowsError.ERROR_BAD_PATHNAME.GetException();

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

                // CreateDirectory will refuse paths that are over MAX_PATH - 12, so we always want to add the prefix
                subDirectory = Paths.AddExtendedPrefix(subDirectory, addIfUnderLegacyMaxPath: true);
                var info = Storage.TryGetFileInfo(Paths.AddExtendedPrefix(subDirectory));
                if (!info.HasValue)
                {
                    // Doesn't exist, try to create it
                    Storage.CreateDirectory(subDirectory);
                }
                else if ((info.Value.FileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // Directory exists, move on
                    continue;
                }
                else
                {
                    // File exists
                    throw WindowsError.ERROR_FILE_EXISTS.GetException(subDirectory);
                }
            }
        }

        public void DeleteFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            Storage.DeleteFile(Paths.AddExtendedPrefix(NormalizeIfNotExtended(path)));
        }

        private void DeleteDirectoryRecursive(string path)
        {
            var info = Storage.TryGetFileInfo(path);
            if (!info.HasValue)
            {
                // Nothing found
                WindowsError.ERROR_PATH_NOT_FOUND.GetException(path);
            }

            if ((info.Value.FileAttributes & FileAttributes.Directory) != FileAttributes.Directory)
            {
                // Not a directory, a file
                throw WindowsError.ERROR_FILE_EXISTS.GetException(path);
            }

            //if (attributes.HasFlag(FileAttributes.ReadOnly))
            //{
            //    // Make it writable
            //    NativeMethods.FileManagement.SetFileAttributes(path, attributes & ~FileAttributes.ReadOnly);
            //}

            if ((info.Value.FileAttributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
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
                    DeleteFile(file.Path);
                }

                foreach (var directory in DirectoryInformation.EnumerateChildrenInternal(
                    directory: path,
                    childType: ChildType.Directory,
                    searchPattern: "*",
                    searchOption: System.IO.SearchOption.TopDirectoryOnly,
                    excludeAttributes: 0,
                    fileService: this))
                {
                    DeleteDirectoryRecursive(directory.Path);
                }
            }

            // We've either emptied or we're a reparse point, delete the directory
            Storage.RemoveDirectory(path);
        }

        public void DeleteDirectory(string path, bool deleteChildren = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Always add the prefix to ensure we can delete Posix names (end in space/period)
            path = Paths.AddExtendedPrefix(NormalizeIfNotExtended(path), addIfUnderLegacyMaxPath: true);

            if (deleteChildren)
            {
                DeleteDirectoryRecursive(path);
            }
            else
            {
                Storage.RemoveDirectory(path);
            }
        }

        public string GetFullPath(string path, string basePath = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Don't mess with \\?\
            if (Paths.IsExtended(path)) return path;

            if (basePath != null && Paths.IsPartiallyQualified(basePath))
                throw new ArgumentException(nameof(basePath));

            switch (Paths.GetPathFormat(path))
            {
                case PathFormat.LocalDriveRooted:
                    // Get the directory for the specified drive, and remove the drive specifier
                    string drive = path.Substring(0, 2);

                    if (basePath == null || !basePath.StartsWith(drive, StringComparison.OrdinalIgnoreCase))
                        // No basepath or it doesn't match, find the current directory for the drive
                        basePath = _directory.GetCurrentDirectory(Paths.AddTrailingSeparator(drive));

                    path = path.Substring(2);

                    break;
                case PathFormat.LocalCurrentDriveRooted:
                    // Get the root directory of the basePath if possible
                    basePath = basePath ?? _directory.GetCurrentDirectory();
                    basePath = Paths.GetRoot(basePath) ?? basePath;
                    break;
            }

            if (basePath == null || !Paths.IsPartiallyQualified(path))
            {
                // Fixed, or we don't have a base path
                return Storage.GetFullPathName(path);
            }
            else
            {
                return Storage.GetFullPathName(Paths.Combine(basePath, path));
            }
        }

        public IFileSystemInformation GetPathInfo(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return FileSystemInformation.Create(NormalizeIfNotExtended(path), this);
        }

        public System.IO.FileAttributes GetAttributes(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return (System.IO.FileAttributes)Storage.GetFileAttributesExtended(Paths.AddExtendedPrefix(NormalizeIfNotExtended(path))).FileAttributes;
        }

        public void SetAttributes(string path, System.IO.FileAttributes attributes)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            Storage.SetFileAttributes(Paths.AddExtendedPrefix(NormalizeIfNotExtended(path)), (FileAttributes)attributes);
        }

        public void CopyFile(string existingPath, string newPath, bool overwrite = false)
        {
            if (existingPath == null) throw new ArgumentNullException(nameof(existingPath));
            if (newPath == null) throw new ArgumentNullException(nameof(newPath));

            Storage.CopyFile(
                Paths.AddExtendedPrefix(NormalizeIfNotExtended(existingPath)),
                Paths.AddExtendedPrefix(NormalizeIfNotExtended(newPath)),
                overwrite);
        }

        private string NormalizeIfNotExtended(string path)
        {
            return Paths.IsExtended(path) ? path : GetFullPath(path);
        }
    }
}
