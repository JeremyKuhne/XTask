// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Attempts to delete all specified paths when disposed
    /// </summary>
    public class FileCleaner : IDisposable
    {
        protected const string XTaskFlagFileName = @"%XTaskFlagFile%";
        protected ConcurrentBag<string> filesToClean = new ConcurrentBag<string>();
        private StreamWriter flagFile;
        private string rootTempFolder;
        protected IFileService fileServiceProvider;
        private static object cleanLock;

        static FileCleaner()
        {
            cleanLock = new object();
        }

        /// <param name="tempRootDirectoryName">The subdirectory to use for temp files "MyApp"</param>
        public FileCleaner(string tempRootDirectoryName, IFileService fileServiceProvider)
        {
            if (String.IsNullOrWhiteSpace(tempRootDirectoryName)) throw new ArgumentNullException("tempRootDirectoryName");
            if (fileServiceProvider == null) throw new ArgumentNullException("fileServiceProvider");

            this.fileServiceProvider = fileServiceProvider;
            this.rootTempFolder = Paths.Combine(Path.GetTempPath(), tempRootDirectoryName);
            this.TempFolder = Paths.Combine(this.rootTempFolder, Path.GetRandomFileName());
            string flagFile = Paths.Combine(this.TempFolder, FileCleaner.XTaskFlagFileName);

            lock (cleanLock)
            {
                // Make sure we fully lock the directory before allowing cleaning
                this.fileServiceProvider.CreateDirectory(this.TempFolder);

                // Create a flag file and leave it open- this way we can track and clean abandoned (crashed/terminated) processes
                Stream flagStream = this.fileServiceProvider.CreateFileStream(flagFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                this.flagFile = new StreamWriter(flagStream);
                this.flagFile.WriteLine(XTaskStrings.FlagFileContent);
                this.flagFile.Flush();
            }
        }

        public string TempFolder { get; private set; }

        public void TrackFile(string path)
        {
            if (!String.IsNullOrEmpty(path) && !path.StartsWith(this.TempFolder, StringComparison.OrdinalIgnoreCase))
            {
                this.filesToClean.Add(path);
            }
        }

        protected virtual void CleanOrphanedTempFolders()
        {
            // Clean up orphaned temp folders
            IDirectoryInformation rootInfo = this.fileServiceProvider.GetPathInfo(this.rootTempFolder) as IDirectoryInformation;

            if (rootInfo != null)
            {
                try
                {
                    var flagFiles = 
                        from directory in rootInfo.EnumerateDirectories()
                        from file in directory.EnumerateFiles(FileCleaner.XTaskFlagFileName)
                        select new { Directory = directory.Path, File = file.Path };

                    foreach (var flagFile in flagFiles.ToArray())
                    {
                        try
                        {
                            // If we can't delete the flag file (open handle) we'll throw and move on
                            this.fileServiceProvider.DeleteFile(flagFile.File);
                            this.fileServiceProvider.DeleteDirectory(flagFile.Directory, deleteChildren: true);
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
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
        }

        protected virtual bool ThrowOnCleanSelf
        {
            get { return false; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (cleanLock)
                {
                    this.flagFile.Dispose();
                    this.flagFile = null;

                    // Delete our own temp folder
                    try
                    {
                        this.fileServiceProvider.DeleteDirectory(this.TempFolder, deleteChildren: true);
                    }
                    catch (Exception)
                    {
                        if (ThrowOnCleanSelf) throw;
                    }

                    // Clean any loose files we're tracking
                    foreach (string file in this.filesToClean.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        if (String.IsNullOrWhiteSpace(file)) { continue; }

                        try
                        {
                            this.fileServiceProvider.DeleteFile(file);
                        }
                        catch (Exception)
                        {
                            if (ThrowOnCleanSelf) throw;
                        }
                    }

                    this.CleanOrphanedTempFolders();
                }
            }
        }
    }
}