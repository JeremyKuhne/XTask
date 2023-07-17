// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES;
using System;

#if NETFRAMEWORK
using IO = Microsoft.IO;
using Microsoft.IO.Enumeration;
#else
using IO = System.IO;
using System.IO.Enumeration;
#endif

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

        protected FileSystemInformation(IFileService fileService) => FileService = fileService;

        protected virtual void PopulateData(ref FileSystemEntry findResult, string directory)
        {
            _source = Source.FindResult;
            Path = IO.Path.Join(directory.AsSpan(), findResult.FileName);
            Attributes = findResult.Attributes;
            CreationTime = findResult.CreationTimeUtc;
            LastAccessTime = findResult.LastAccessTimeUtc;
            LastWriteTime = findResult.LastWriteTimeUtc;
            Name = findResult.FileName.ToString();
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
            => Storage.CreateFile(
                Paths.AddExtendedPrefix(path),
                // We don't care about read or write, we're just getting metadata with this handle
                default,
                System.IO.FileShare.ReadWrite,
                System.IO.FileMode.Open,
                default,
                // To avoid traversing links | to open directories.
                FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS);


        protected virtual void PopulateData(ref FileSystemEntry findData)
        {
            _source = Source.FindResult;
            Name = findData.FileName.ToString();
            Path = IO.Path.Join(findData.Directory, Name.AsSpan());
            Attributes = findData.Attributes;
            CreationTime = findData.CreationTimeUtc;
            LastAccessTime = findData.LastAccessTimeUtc;
            LastWriteTime = findData.LastWriteTimeUtc;
            Exists = true;
        }

        protected virtual void PopulateData(string originalPath, SafeFileHandle fileHandle, FILE_BASIC_INFO info)
        {
            _source = Source.FileInfo;

            string finalPath = originalPath;

            try
            {
                string originalRoot = Paths.GetRoot(originalPath);
                finalPath = Storage.GetFinalPathNameByHandle(fileHandle, GETFINALPATHNAMEBYHANDLE_FLAGS.FILE_NAME_NORMALIZED);

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
            Attributes = (System.IO.FileAttributes)info.FileAttributes;
            CreationTime = DateTimeOffset.FromFileTime(info.CreationTime);
            LastAccessTime = DateTimeOffset.FromFileTime(info.LastAccessTime);
            LastWriteTime = DateTimeOffset.FromFileTime(info.LastWriteTime);
            Name = Paths.GetFileOrDirectoryName(finalPath) ?? finalPath;
            Exists = true;
        }

        internal static IFileSystemInformation Create(ref FileSystemEntry findData, IFileService fileService)
        {
            if (findData.Attributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                return DirectoryInformation.Create(ref findData, fileService);
            }
            else
            {
                return FileInformation.Create(ref findData, fileService);
            }
        }

        internal static IFileSystemInformation Create(string path, IFileService fileService)
        {
            using SafeFileHandle fileHandle = GetFileHandle(path);
            var info = Storage.GetFileBasicInfo(fileHandle);
            return Create(path, fileHandle, info, fileService);
        }

        internal static IFileSystemInformation Create(
            string originalPath,
            SafeFileHandle fileHandle,
            FILE_BASIC_INFO info,
            IFileService fileService)
        {
            if (((FILE_FLAGS_AND_ATTRIBUTES)info.FileAttributes).HasFlag(FILE_ATTRIBUTE_DIRECTORY))
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

        public System.IO.FileAttributes Attributes { get; private set; }

        public DateTimeOffset CreationTime { get; private set; }

        public DateTimeOffset LastAccessTime { get; private set; }

        public DateTimeOffset LastWriteTime { get; private set; }

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
                        System.IO.FileAttributes attributes =
                            (System.IO.FileAttributes)Storage.GetFileInfo(Paths.AddExtendedPrefix(Path)).dwFileAttributes;
                        PopulateData(Path, attributes);
                        break;
                    case Source.FindResult:
                    case Source.FileInfo:
                        using (SafeFileHandle fileHandle = GetFileHandle(Path))
                        {
                            PopulateData(Path, fileHandle, Storage.GetFileBasicInfo(fileHandle));
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
