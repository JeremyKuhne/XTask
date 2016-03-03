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
        private IExtendedFileService _extendedFileService;
        private IFileService _fileService;
        private Dictionary<string, string> _volumeDirectories = new Dictionary<string, string>();
        private string _lastVolume;

        public CurrentDirectory(IFileService fileService, IExtendedFileService extendedFileService)
        {
            _fileService = fileService;
            _extendedFileService = extendedFileService;
            SetCurrentDirectory(Environment.CurrentDirectory);
        }

        public void SetCurrentDirectory(string directory)
        {
            if (Paths.IsPartiallyQualified(directory))
            {
                throw new ArgumentException("Argument cannot be relative", nameof(directory));
            }

            _lastVolume = AddEntry(directory);
        }

        private string AddEntry(string directory, string canonicalRoot = null)
        {
            string root = Paths.GetRoot(directory);
            canonicalRoot = canonicalRoot ?? _extendedFileService.GetCanonicalRoot(_fileService, directory);

            // If the directory has vanished, walk up
            while (!_fileService.DirectoryExists(directory)
                && !string.Equals((directory = Paths.GetDirectory(directory)), root, StringComparison.Ordinal))
            {
                Debug.Assert(directory != null);
            }

            if (_volumeDirectories.ContainsKey(canonicalRoot))
            {
                _volumeDirectories[canonicalRoot] = directory;
            }
            else
            {
                _volumeDirectories.Add(canonicalRoot, directory);
            }

            return canonicalRoot;
        }

        public string GetCurrentDirectory(string path = null)
        {
            string volume = path == null ? _lastVolume : _extendedFileService.GetCanonicalRoot(_fileService, path);

            string directory;
            if (_volumeDirectories.TryGetValue(volume, out directory))
            {
                AddEntry(directory, volume);
                return directory;
            }
            else
            {
                // Try to get the hidden environment variable (e.g. "=C:") for the given drive if available
                string driveLetter = _extendedFileService.GetDriveLetter(_fileService, path);
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
