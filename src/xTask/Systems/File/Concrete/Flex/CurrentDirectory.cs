// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.Flex
{
    using System;
    using System.Collections.Generic;
    using Interop;
    using XTask.Systems.File;
    using System.Diagnostics;

    public class CurrentDirectory
    {
        private IExtendedFileService fileService;

        public CurrentDirectory(IExtendedFileService fileService)
        {
            this.fileService = fileService;
            this.SetCurrentDirectory(Environment.CurrentDirectory);
        }

        private Dictionary<string, string> volumeDirectories = new Dictionary<string, string>();
        private string lastVolume;

        public void SetCurrentDirectory(string directory)
        {
            if (Paths.IsRelative(directory))
            {
                throw new InvalidOperationException();
            }

            this.lastVolume = AddEntry(directory);
        }

        private string AddEntry(string directory, string canonicalRoot = null)
        {
            canonicalRoot = canonicalRoot ?? fileService.GetCanonicalRoot(directory);

            // If the directory has vanished, walk up
            while (!fileService.DirectoryExists(directory)
                && !String.Equals((directory = Paths.GetDirectory(directory)), canonicalRoot, StringComparison.Ordinal))
            {
                Debug.Assert(directory != null);
            }

            if (this.volumeDirectories.ContainsKey(canonicalRoot))
            {
                this.volumeDirectories[canonicalRoot] = directory;
            }
            else
            {
                this.volumeDirectories.Add(canonicalRoot, directory);
            }

            return canonicalRoot;
        }

        public string GetCurrentDirectory(string path = null)
        {
            string volume = path == null ? lastVolume : fileService.GetCanonicalRoot(path);

            string directory;
            if (this.volumeDirectories.TryGetValue(volume, out directory))
            {
                AddEntry(directory, volume);
                return directory;
            }
            else
            {
                // Try to get the hidden environment variable (e.g. "=C:") for the given drive if available
                string driveLetter = fileService.GetDriveLetter(path);
                if (driveLetter != null)
                {
                    string environmentPath = NativeMethods.GetEnvironmentVariable("=" + driveLetter.Substring(0, 2));
                    if (environmentPath != null)
                    {
                        AddEntry(environmentPath);
                        return environmentPath;
                    }
                }

                // Nothing is set yet, assume the root
                string root = Paths.GetRoot(path);
                AddEntry(root);
                return root;
            }
        }
    }
}
