// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES;
using xTask.Utility;
using Windows.Win32;
using static XTask.Systems.File.Concrete.Flex.FileSystemInformation;
using Windows.Support;

namespace XTask.Systems.File.Concrete.Flex;

/// <summary>
///  File service that provides fast and accurate file system access. Supports extended syntax and implicit long paths.
///  Maintains a separate, enhanced current directory that supports alternate volume names.
/// </summary>
/// <remarks>
///  Also supports UNCs, devices, and alternate data streams.
/// </remarks>
internal class FileService : IFileService
{
    private readonly CurrentDirectory _directory;

    public FileService(IExtendedFileService extendedFileService, string initialCurrentDirectory = null)
    {
        _directory = new CurrentDirectory(this, extendedFileService, initialCurrentDirectory);
    }

    public string CurrentDirectory
    {
        get => _directory.GetCurrentDirectory();
        set => _directory.SetCurrentDirectory(value);
    }

    public System.IO.Stream CreateFileStream(
        string path,
        System.IO.FileMode mode = System.IO.FileMode.Open,
        System.IO.FileAccess access = System.IO.FileAccess.Read,
        System.IO.FileShare share = System.IO.FileShare.ReadWrite)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));

        path = Paths.AddExtendedPrefix(NormalizeIfNotExtended(path));

        // Flags match what FileStream does internally
        return Storage.CreateFileStream(
            path,
            access,
            share,
            mode,
            fileAttributes: 0,
            flagsAndAttributes: SECURITY_SQOS_PRESENT | SECURITY_ANONYMOUS);
    }

    public void CreateDirectory(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        path = GetFullPath(path);
        int pathRootLength = Paths.GetRootLength(path);
        if (pathRootLength < 0)
        {
            WIN32_ERROR.ERROR_BAD_PATHNAME.Throw();
        }

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
            if (!Storage.TryGetFileInfo(Paths.AddExtendedPrefix(subDirectory), out var data))
            {
                // Doesn't exist, try to create it
                Storage.CreateDirectory(subDirectory);
            }
            else if (((FILE_FLAGS_AND_ATTRIBUTES)data.dwFileAttributes).HasFlag(FILE_ATTRIBUTE_DIRECTORY))
            {
                // Directory exists, move on
                continue;
            }
            else
            {
                // File exists
                WIN32_ERROR.ERROR_FILE_EXISTS.Throw(subDirectory);
            }
        }
    }

    public void DeleteFile(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        Interop.DeleteFile(Paths.AddExtendedPrefix(NormalizeIfNotExtended(path))).ThrowLastErrorIfFalse();
    }

    private void DeleteDirectoryRecursive(string path)
    {
        if (!Storage.TryGetFileInfo(path, out var data))
        {
            WIN32_ERROR.ERROR_PATH_NOT_FOUND.Throw(path);
        }

        if (!((FILE_FLAGS_AND_ATTRIBUTES)data.dwFileAttributes).HasFlag(FILE_ATTRIBUTE_DIRECTORY))
        {
            // Not a directory, a file
            WIN32_ERROR.ERROR_FILE_EXISTS.Throw(path);
        }

        if (!((FILE_FLAGS_AND_ATTRIBUTES)data.dwFileAttributes).HasFlag(FILE_ATTRIBUTE_REPARSE_POINT))
        {
            // Remove the subdirectories and files
            // Reparse points are simply disconnected, they don't need to be emptied.
            foreach (var file in DirectoryInformation.EnumerateChildrenInternal(
                directory: path,
                childType: ChildType.File,
                searchPattern: "*",
                searchOption: System.IO.SearchOption.TopDirectoryOnly,
                excludeAttributes: default,
                fileService: this))
            {
                DeleteFile(file.Path);
            }

            foreach (var directory in DirectoryInformation.EnumerateChildrenInternal(
                directory: path,
                childType: ChildType.Directory,
                searchPattern: "*",
                searchOption: System.IO.SearchOption.TopDirectoryOnly,
                excludeAttributes: default,
                fileService: this))
            {
                DeleteDirectoryRecursive(directory.Path);
            }
        }

        // We've either emptied or we're a reparse point, delete the directory
        Interop.RemoveDirectory(path).ThrowLastErrorIfFalse(path);
    }

    public void DeleteDirectory(string path, bool deleteChildren = false)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));

        // Always add the prefix to ensure we can delete POSIX names (end in space/period)
        path = Paths.AddExtendedPrefix(NormalizeIfNotExtended(path), addIfUnderLegacyMaxPath: true);

        if (deleteChildren)
        {
            DeleteDirectoryRecursive(path);
        }
        else
        {
            Interop.RemoveDirectory(path).ThrowLastErrorIfFalse(path);
        }
    }

    public unsafe string GetFullPath(string path, string basePath = null)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));

        // Don't mess with \\?\
        if (Paths.IsExtended(path)) return path;

        if (basePath is not null && Paths.IsPartiallyQualified(basePath))
            throw new ArgumentException(nameof(basePath));

        switch (Paths.GetPathFormat(path))
        {
            case PathFormat.LocalDriveRooted:
                // Get the directory for the specified drive, and remove the drive specifier
                string drive = path.Substring(0, 2);

                if (basePath is null || !basePath.StartsWith(drive, StringComparison.OrdinalIgnoreCase))
                {
                    // No basepath or it doesn't match, find the current directory for the drive
                    basePath = _directory.GetCurrentDirectory(Paths.EnsureTrailingSeparator(drive));
                }

                path = path.Substring(2);

                break;
            case PathFormat.LocalCurrentDriveRooted:
                // Get the root directory of the basePath if possible
                basePath ??= _directory.GetCurrentDirectory();
                basePath = Paths.GetRoot(basePath) ?? basePath;
                break;
        }

        if (basePath is not null && Paths.IsPartiallyQualified(path))
        {
            path = Paths.Combine(basePath, path);
        }

        using BufferScope<char> buffer = new(stackalloc char[260]);
        while (true)
        {
            fixed (char* b = buffer)
            {
                uint count = Interop.GetFullPathName(path, (uint)buffer.Length, b, null);

                if (count == 0)
                {
                    Error.ThrowLastError(path);
                }

                if (count < buffer.Length)
                {
                    return buffer.Slice(0, (int)count).ToString();
                }

                buffer.EnsureCapacity((int)count);
            }
        }
    }

    public IFileSystemInformation GetPathInfo(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        return Create(NormalizeIfNotExtended(path), this);
    }

    public unsafe System.IO.FileAttributes GetAttributes(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        string extendedPath = Paths.AddExtendedPrefix(NormalizeIfNotExtended(path));
        WIN32_FILE_ATTRIBUTE_DATA data;
        Interop.GetFileAttributesEx(extendedPath, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, &data).ThrowLastErrorIfFalse(path);
        return (System.IO.FileAttributes)data.dwFileAttributes;
    }

    public void SetAttributes(string path, System.IO.FileAttributes attributes)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        string extendedath = Paths.AddExtendedPrefix(NormalizeIfNotExtended(path));
        Interop.SetFileAttributes(extendedath, (FILE_FLAGS_AND_ATTRIBUTES)attributes).ThrowLastErrorIfFalse(path);
    }

    public unsafe void CopyFile(string existingPath, string newPath, bool overwrite = false)
    {
        if (existingPath is null) throw new ArgumentNullException(nameof(existingPath));
        if (newPath is null) throw new ArgumentNullException(nameof(newPath));

        BOOL cancel;
        COPYFILE2_EXTENDED_PARAMETERS parameters = new()
        {
            dwSize = (uint)sizeof(COPYFILE2_EXTENDED_PARAMETERS),
            pfCancel = &cancel,
            dwCopyFlags = overwrite ? 0u : Interop.COPY_FILE_FAIL_IF_EXISTS
        };

        Interop.CopyFile2(
            Paths.AddExtendedPrefix(NormalizeIfNotExtended(existingPath)),
            Paths.AddExtendedPrefix(NormalizeIfNotExtended(newPath)),
            parameters).ThrowOnFailure();
    }

    private string NormalizeIfNotExtended(string path) => Paths.IsExtended(path) ? path : GetFullPath(path);
}
