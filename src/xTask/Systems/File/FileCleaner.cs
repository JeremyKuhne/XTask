﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.IO;

namespace XTask.Systems.File;

/// <summary>
///  Attempts to delete all specified paths when disposed
/// </summary>
public class FileCleaner : IDisposable
{
    protected const string XTaskFlagFileName = @"%XTaskFlagFile%";
    protected ConcurrentBag<string> _filesToClean = new();
    private StreamWriter _flagFile;
    private readonly string _rootTempFolder;
    protected IFileService _fileServiceProvider;
    private static readonly object s_CleanLock = new();

    /// <param name="tempRootDirectoryName">The subdirectory to use for temp files "MyApp"</param>
    public FileCleaner(string tempRootDirectoryName, IFileService fileServiceProvider)
    {
        if (string.IsNullOrWhiteSpace(tempRootDirectoryName)) throw new ArgumentNullException(nameof(tempRootDirectoryName));
        if (fileServiceProvider is null) throw new ArgumentNullException(nameof(fileServiceProvider));

        _fileServiceProvider = fileServiceProvider;
        _rootTempFolder = Paths.Combine(Path.GetTempPath(), tempRootDirectoryName);
        TempFolder = Paths.Combine(_rootTempFolder, Path.GetRandomFileName());
        string flagFile = Paths.Combine(TempFolder, XTaskFlagFileName);

        lock (s_CleanLock)
        {
            // Make sure we fully lock the directory before allowing cleaning
            _fileServiceProvider.CreateDirectory(TempFolder);

            // Create a flag file and leave it open- this way we can track and clean abandoned (crashed/terminated) processes
            Stream flagStream = _fileServiceProvider.CreateFileStream(flagFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            _flagFile = new StreamWriter(flagStream);
            _flagFile.WriteLine(XTaskStrings.FlagFileContent);
            _flagFile.Flush();
        }
    }

    public string TempFolder { get; private set; }

    public void TrackFile(string path)
    {
        if (!string.IsNullOrEmpty(path) && !path.StartsWith(TempFolder, StringComparison.OrdinalIgnoreCase))
        {
            _filesToClean.Add(path);
        }
    }

    protected virtual void CleanOrphanedTempFolders()
    {
        // Clean up orphaned temp folders
        if (_fileServiceProvider.GetPathInfo(_rootTempFolder) is not IDirectoryInformation rootInfo)
        {
            return;
        }

        try
        {
            var flagFiles =
                from directory in rootInfo.EnumerateDirectories()
                from file in directory.EnumerateFiles(XTaskFlagFileName)
                select new { Directory = directory.Path, File = file.Path };

            foreach (var flagFile in flagFiles.ToArray())
            {
                try
                {
                    // If we can't delete the flag file (open handle) we'll throw and move on
                    _fileServiceProvider.DeleteFile(flagFile.File);
                    _fileServiceProvider.DeleteDirectory(flagFile.Directory, deleteChildren: true);
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
            // Ignoring orphan cleanup errors as the DotNet file service chokes on long paths
        }
    }

    public void Dispose() => Dispose(disposing: true);

    protected virtual bool ThrowOnCleanSelf => false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        lock (s_CleanLock)
        {
            _flagFile.Dispose();
            _flagFile = null;

            // Delete our own temp folder
            try
            {
                _fileServiceProvider.DeleteDirectory(TempFolder, deleteChildren: true);
            }
            catch (Exception)
            {
                if (ThrowOnCleanSelf) throw;
            }

            // Clean any loose files we're tracking
            foreach (string file in _filesToClean.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(file)) { continue; }

                try
                {
                    _fileServiceProvider.DeleteFile(file);
                }
                catch (Exception)
                {
                    if (ThrowOnCleanSelf) throw;
                }
            }

            CleanOrphanedTempFolders();
        }
    }
}