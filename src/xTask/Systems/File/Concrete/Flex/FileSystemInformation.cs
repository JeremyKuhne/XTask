// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using WInterop.FileManagement;
using WInterop.FileManagement.DataTypes;
using WInterop.Handles.DataTypes;

namespace XTask.Systems.File.Concrete.Flex
{
    internal class FileSystemInformation : IFileSystemInformation
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

        protected virtual void PopulateData(FindResult findResult, string directory)
        {
            _source = Source.FindResult;
            Path = Paths.Combine(directory, findResult.FileName);
            Attributes = (System.IO.FileAttributes)findResult.Attributes;
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
            return FileMethods.CreateFile(
                Paths.AddExtendedPrefix(path),
                // We don't care about read or write, we're just getting metadata with this handle
                0,
                ShareMode.FILE_SHARE_READWRITE,
                CreationDisposition.OPEN_EXISTING,
                FileAttributes.NONE,
                FileFlags.FILE_FLAG_OPEN_REPARSE_POINT          // To avoid traversing links
                    | FileFlags.FILE_FLAG_BACKUP_SEMANTICS);    // To open directories
        }

        protected virtual void PopulateData(string originalPath, SafeFileHandle fileHandle, FileBasicInfo info)
        {
            _source = Source.FileInfo;

            string finalPath = originalPath;

            try
            {
                string originalRoot = Paths.GetRoot(originalPath);
                finalPath = FileDesktopMethods.GetFinalPathNameByHandle(fileHandle, GetFinalPathNameByHandleFlags.FILE_NAME_NORMALIZED);

                // GetFinalPathNameByHandle will use the legacy drive for the volume (e.g. \\?\C:\). We may have started with C:\ or some other
                // volume name format (\\?\Volume({GUID}), etc.) and we want to put the original volume specifier back.
                finalPath = Paths.ReplaceCasing(finalPath, originalPath);
            }
            catch
            {
                if (originalPath.IndexOf(@"pipe", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    // Getting the final path name doesn't work with the pipes device. Not sure if there is a programmatic way
                    // to know if an arbitrary file handle won't work with GetFinalPathName- potentially may be other cases.
                    throw;
                }
            }

            _source = Source.FindResult;
            Path = finalPath;
            Attributes = (System.IO.FileAttributes)info.Attributes;
            CreationTime = info.CreationTime;
            LastAccessTime = info.LastAccessTime;
            LastWriteTime = info.LastWriteTime;
            Name = Paths.GetFileOrDirectoryName(finalPath) ?? finalPath;
            Exists = true;
        }

        internal static IFileSystemInformation Create(FindResult findResult, string directory, IFileService fileService)
        {
            if ((findResult.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != 0)
            {
                return DirectoryInformation.Create(findResult, directory, fileService);
            }
            else
            {
                return FileInformation.Create(findResult, directory, fileService);
            }
        }

        internal static IFileSystemInformation Create(string path, IFileService fileService)
        {
            using (SafeFileHandle fileHandle = GetFileHandle(path))
            {
                var info = FileMethods.GetFileBasicInfoByHandle(fileHandle);
                return Create(path, fileHandle, info, fileService);
            }
        }

        internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, FileBasicInfo info, IFileService fileService)
        {
            if ((info.Attributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != 0)
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
                        System.IO.FileAttributes attributes = (System.IO.FileAttributes)FileMethods.GetFileAttributesEx(Paths.AddExtendedPrefix(Path)).Attributes;
                        PopulateData(Path, attributes);
                        break;
                    case Source.FindResult:
                    case Source.FileInfo:
                        using (SafeFileHandle fileHandle = GetFileHandle(Path))
                        {
                            var info = FileMethods.GetFileBasicInfoByHandle(fileHandle);
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
