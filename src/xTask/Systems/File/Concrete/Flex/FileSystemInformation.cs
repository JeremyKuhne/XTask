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

    internal class FileSystemInformation : IFileSystemInformation, IExtendedFileSystemInformation
    {
        internal enum Source : byte
        {
            Attributes,
            FindResult,
            FileInfo
        }

        protected Source _source;

        protected IFileService FileService { get; private set; }

        protected FileSystemInformation(IFileService fileService)
        {
            FileService = fileService;
        }

        protected virtual void PopulateData(NativeMethods.FileManagement.FindResult findResult)
        {
            _source = Source.FindResult;
            Path = Paths.Combine(findResult.BasePath, findResult.FileName);
            Attributes = findResult.Attributes;
            CreationTime = findResult.Creation;
            LastAccessTime = findResult.LastAccess;
            LastWriteTime = findResult.LastWrite;
            Name = findResult.FileName;
            Exists = true;
        }

        protected void PopulateData(string path, System.IO.FileAttributes attributes)
        {
            _source = Source.Attributes;
            Path = path;
            Name = path;
            Attributes = attributes;
            Exists = true;
        }

        private static SafeFileHandle GetFileHandle(string path)
        {
            return NativeMethods.FileManagement.CreateFile(
                path,
                0,                  // We don't care about read or write, we're just getting metadata with this handle
                System.IO.FileShare.ReadWrite,
                System.IO.FileMode.Open,
                NativeMethods.FileManagement.AllFileAttributeFlags.FILE_ATTRIBUTE_NORMAL
                    | NativeMethods.FileManagement.AllFileAttributeFlags.FILE_FLAG_OPEN_REPARSE_POINT   // To avoid traversing links
                    | NativeMethods.FileManagement.AllFileAttributeFlags.FILE_FLAG_BACKUP_SEMANTICS);   // To open directories
        }

        protected virtual void PopulateData(string originalPath, SafeFileHandle fileHandle, NativeMethods.FileManagement.BY_HANDLE_FILE_INFORMATION info)
        {
            _source = Source.FileInfo;

            string originalRoot = Paths.GetRoot(originalPath);
            string finalPath = NativeMethods.FileManagement.GetFinalPathName(fileHandle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED);
            finalPath = Paths.ReplaceRoot(originalPath, finalPath);

            _source = Source.FindResult;
            Path = finalPath;
            Attributes = info.dwFileAttributes;
            CreationTime = NativeMethods.GetDateTime(info.ftCreationTime);
            LastAccessTime = NativeMethods.GetDateTime(info.ftLastAccessTime);
            LastWriteTime = NativeMethods.GetDateTime(info.ftLastWriteTime);
            Name = Paths.GetFileOrDirectoryName(finalPath) ?? finalPath;
            FileIndex = NativeMethods.HighLowToLong(info.nFileIndexHigh, info.nFileSizeLow);
            NumberOfLinks = info.nNumberOfLinks;
            VolumeSerialNumber = info.dwVolumeSerialNumber;
            Exists = true;
        }

        internal static IFileSystemInformation Create(NativeMethods.FileManagement.FindResult findResult, IFileService fileService)
        {
            if ((findResult.Attributes & System.IO.FileAttributes.Directory) != 0)
            {
                return DirectoryInformation.Create(findResult, fileService);
            }
            else
            {
                return FileInformation.Create(findResult, fileService);
            }
        }

        internal static IFileSystemInformation Create(string path, IFileService fileService)
        {
            using (SafeFileHandle fileHandle = GetFileHandle(path))
            {
                string canonicalPath = NativeMethods.FileManagement.GetFinalPathName(fileHandle, 0);

                // GetFinalPathNameByHandle will use the legacy drive for the volume (e.g. \\?\C:\). We may have started with C:\ or some other
                // volume name format (\\?\Volume({GUID}), etc.) and we want to put the original volume specifier back.
                canonicalPath = Paths.ReplaceRoot(path, canonicalPath);

                NativeMethods.FileManagement.BY_HANDLE_FILE_INFORMATION info = NativeMethods.FileManagement.GetFileInformationByHandle(fileHandle);
                return Create(canonicalPath, fileHandle, info, fileService);
            }
        }

        internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, NativeMethods.FileManagement.BY_HANDLE_FILE_INFORMATION info, IFileService fileService)
        {
            if ((info.dwFileAttributes & System.IO.FileAttributes.Directory) != 0)
            {
                return DirectoryInformation.Create(originalPath, fileHandle, info, fileService);
            }
            else
            {
                return FileInformation.Create(originalPath, fileHandle, info, fileService);
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
                // Should only be using attributes for root directories
                throw new InvalidOperationException();
            }
        }

        //internal static IFileSystemInformation Create(string path, IFileService fileService)
        //{
        //    path = fileService.GetFullPath(path);

        //    try
        //    {
        //        var findResult = NativeMethods.FileManagement.FindFirstFile(path, directoriesOnly: false, getAlternateName: false, returnNullIfNotFound: false);
        //        var info = Create(findResult, fileService);
        //        findResult.FindHandle.Close();
        //        return info;
        //    }
        //    catch (System.IO.IOException)
        //    {
        //        // Could be a root directory (e.g. C:), can't do FindFile
        //        if (Paths.IsPathRelative(path))
        //        {
        //            throw;
        //        }

        //        System.IO.FileAttributes attributes = NativeMethods.FileManagement.GetFileAttributes(path);
        //        return Create(path, attributes, fileService);
        //    }
        //}

        public System.IO.FileAttributes Attributes { get; private set; }

        public DateTime CreationTime { get; private set; }

        public DateTime LastAccessTime { get; private set; }

        public DateTime LastWriteTime { get; private set; }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public bool Exists { get; private set; }

        //
        // Extended info
        //

        public uint VolumeSerialNumber { get; private set; }

        public uint NumberOfLinks { get; private set; }

        public ulong FileIndex { get; private set; }

        public virtual void Refresh()
        {
            try
            {
                switch (_source)
                {
                    case Source.Attributes:
                        System.IO.FileAttributes attributes = NativeMethods.FileManagement.GetFileAttributes(Path);
                        PopulateData(Path, attributes);
                        break;
                    case Source.FindResult:
                        var findResult = NativeMethods.FileManagement.FindFirstFile(Path);
                        PopulateData(findResult);
                        findResult.FindHandle.Close();
                        break;
                    case Source.FileInfo:
                        using (SafeFileHandle fileHandle = GetFileHandle(Path))
                        {
                            NativeMethods.FileManagement.BY_HANDLE_FILE_INFORMATION info = NativeMethods.FileManagement.GetFileInformationByHandle(fileHandle);
                            PopulateData(Path, fileHandle, info);
                        }
                        break;
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Exists = false;
            }
        }
    }
}
