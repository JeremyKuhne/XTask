// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace XTask.Systems.File.Concrete.DotNet;

public abstract class FileSystemInformation : IFileSystemInformation
{
    private readonly FileSystemInfo _fileSystemInfo;
    protected IFileService FileService { get; private set; }

    protected FileSystemInformation(FileSystemInfo fileSystemInfo, IFileService fileService)
    {
        FileService = fileService;
        _fileSystemInfo = fileSystemInfo;
    }

    public virtual string Path => _fileSystemInfo.FullName;
    public virtual string Name => _fileSystemInfo.Name;
    public virtual bool Exists => _fileSystemInfo.Exists;
    public virtual DateTimeOffset CreationTime => _fileSystemInfo.CreationTime;
    public virtual DateTimeOffset LastAccessTime => _fileSystemInfo.LastAccessTime;
    public virtual DateTimeOffset LastWriteTime => _fileSystemInfo.LastWriteTime;

    public virtual FileAttributes Attributes
    {
        get => _fileSystemInfo.Attributes;
        set => _fileSystemInfo.Attributes = value;
    }

    public virtual void Refresh() => _fileSystemInfo.Refresh();
}
