// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using System;
using WInterop.Storage;

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
            CreationTime = findResult.CreationUtc;
            LastAccessTime = findResult.LastAccessUtc;
            LastWriteTime = findResult.LastWriteUtc;
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
            return Storage.CreateFile(
                Paths.AddExtendedPrefix(path),
                CreationDisposition.OpenExisting,
                // We don't care about read or write, we're just getting metadata with this handle
                0,
                ShareModes.ReadWrite,
                FileAttributes.None,
                FileFlags.OpenReparsePoint           // To avoid traversing links
                    | FileFlags.BackupSemantics);    // To open directories
        }


        protected virtual void PopulateData(ref RawFindData findData)
        {
            _source = Source.FindResult;
            Name = findData.FileName.ToString();
            Path = Paths.Combine(findData.Directory, Name);
            Attributes = (System.IO.FileAttributes)findData.FileAttributes;
            CreationTime = findData.CreationTimeUtc;
            LastAccessTime = findData.LastAccessTimeUtc;
            LastWriteTime = findData.LastWriteTimeUtc;
            Exists = true;
        }

        protected virtual void PopulateData(string originalPath, SafeFileHandle fileHandle, FileBasicInformation info)
        {
            _source = Source.FileInfo;

            string finalPath = originalPath;

            try
            {
                string originalRoot = Paths.GetRoot(originalPath);
                finalPath = Storage.GetFinalPathNameByHandle(fileHandle, GetFinalPathNameByHandleFlags.FileNameNormalized);

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
            CreationTime = info.CreationTime.ToDateTimeUtc();
            LastAccessTime = info.LastAccessTime.ToDateTimeUtc();
            LastWriteTime = info.LastWriteTime.ToDateTimeUtc();
            Name = Paths.GetFileOrDirectoryName(finalPath) ?? finalPath;
            Exists = true;
        }

        internal static IFileSystemInformation Create(ref RawFindData findData, IFileService fileService)
        {
            if ((findData.FileAttributes & FileAttributes.Directory) != 0)
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
            using (SafeFileHandle fileHandle = GetFileHandle(path))
            {
                var info = Storage.GetFileBasicInformation(fileHandle);
                return Create(path, fileHandle, info, fileService);
            }
        }

        internal static IFileSystemInformation Create(string originalPath, SafeFileHandle fileHandle, FileBasicInformation info, IFileService fileService)
        {
            if ((info.FileAttributes & FileAttributes.Directory) != 0)
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
                            (System.IO.FileAttributes)Storage.GetFileAttributesExtended(Paths.AddExtendedPrefix(Path)).FileAttributes;
                        PopulateData(Path, attributes);
                        break;
                    case Source.FindResult:
                    case Source.FileInfo:
                        using (SafeFileHandle fileHandle = GetFileHandle(Path))
                        {
                            var info = Storage.GetFileBasicInformation(fileHandle);
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
