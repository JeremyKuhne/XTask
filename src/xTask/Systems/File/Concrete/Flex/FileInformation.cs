// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES;
using Windows.Win32;
using Windows.Win32.Foundation;
using xTask.Utility;

#if NETFRAMEWORK
using IO = Microsoft.IO;
using Microsoft.IO.Enumeration;
#else
using IO = System.IO;
using System.IO.Enumeration;
#endif

namespace XTask.Systems.File.Concrete.Flex;

internal class FileInformation : FileSystemInformation, IFileInformation
{
    private byte[] _md5Hash;
    private IDirectoryInformation _directoryInformation;
    private string _directory;

    private FileInformation(IFileService fileService) : base (fileService)
    {
    }

    new static internal IFileSystemInformation Create(ref FileSystemEntry findData, IFileService fileService)
    {
        if (findData.Attributes.HasFlag(System.IO.FileAttributes.Directory))
        {
            throw new ArgumentOutOfRangeException(nameof(findData));
        }

        FileInformation fileInfo = new(fileService);
        fileInfo.PopulateData(ref findData);
        fileInfo.Length = (ulong)findData.Length;
        return fileInfo;
    }

    new internal static IFileSystemInformation Create(
        string originalPath,
        SafeFileHandle fileHandle,
        FILE_BASIC_INFO info,
        IFileService fileService)
    {
        if (((FILE_FLAGS_AND_ATTRIBUTES)info.FileAttributes).HasFlag(FILE_ATTRIBUTE_DIRECTORY))
        {
            throw new ArgumentOutOfRangeException(nameof(info));
        }

        var fileInfo = new FileInformation(fileService);
        fileInfo.PopulateData(originalPath, fileHandle, info);
        return fileInfo;
    }

    protected override void PopulateData(string originalPath, SafeFileHandle fileHandle, FILE_BASIC_INFO info)
    {
        base.PopulateData(originalPath, fileHandle, info);
        Interop.GetFileSizeEx((HANDLE)fileHandle.DangerousGetHandle(), out long size).ThrowLastErrorIfFalse(originalPath);
        Length = (ulong)size;
        _directory = Paths.GetDirectory(Path);
        GC.KeepAlive(fileHandle);
    }

    public ulong Length { get; private set; }

    public IDirectoryInformation Directory
        => _directoryInformation ??= (IDirectoryInformation)Create(_directory, FileService);

    public byte[] MD5Hash => _md5Hash ??= FileService.GetHash(Path);

    public override void Refresh()
    {
        _md5Hash = null;
        _directoryInformation = null;
        base.Refresh();
    }
}
