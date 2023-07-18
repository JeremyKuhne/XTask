// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace XTask.Systems.File.Concrete.DotNet;

using File = System.IO.File;

/// <summary>
///  File service that exclusively uses the .NET IO implementation to back it
/// </summary>
public class FileService : IFileService
{
    public string CurrentDirectory
    {
        get => Environment.CurrentDirectory;
        set => Environment.CurrentDirectory = value;
    }

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public Stream CreateFileStream(
        string path,
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.ReadWrite) => new FileStream(path, mode, access, share);

    public void DeleteDirectory(string path, bool deleteChildren = false)
        => Directory.Delete(path, recursive: deleteChildren);

    public void DeleteFile(string path) => File.Delete(path);

    public IFileSystemInformation GetPathInfo(string path)
    {
        FileAttributes attributes = File.GetAttributes(path);
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            return new DirectoryInformation(new DirectoryInfo(path), this);
        }
        else
        {
            return new FileInformation(new FileInfo(path), this);
        }
    }

    public string GetFullPath(string path, string basePath = null)
    {
        if (basePath is null || !Paths.IsPartiallyQualified(path))
        {
            // Fixed, or we don't have a base path
            return Path.GetFullPath(path);
        }
        else
        {
            return Path.GetFullPath(Paths.Combine(basePath, path));
        }
    }

    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);

    public void SetAttributes(string path, FileAttributes attributes) => File.SetAttributes(path, attributes);

    public void CopyFile(string existingPath, string newPath, bool overwrite = false)
        => File.Copy(existingPath, newPath, overwrite);
}
