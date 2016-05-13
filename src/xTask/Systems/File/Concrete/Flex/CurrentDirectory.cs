// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using XTask.Interop;

namespace XTask.Systems.File.Concrete.Flex
{
    /// <summary>
    /// Maintains a set of current directories for all volumes. Normalizes volume names.
    /// </summary>
    public class CurrentDirectory
    {
        private IExtendedFileService _extendedFileService;
        private IFileService _fileService;
        private Dictionary<string, string> _volumeDirectories = new Dictionary<string, string>();
        private string _lastVolume;

        public CurrentDirectory(IFileService fileService, IExtendedFileService extendedFileService, string initialCurrentDirectory = null)
        {
            _fileService = fileService;
            _extendedFileService = extendedFileService;
            SetCurrentDirectory(initialCurrentDirectory ?? Environment.CurrentDirectory);
        }

        /// <summary>
        /// Sets the current directory.
        /// </summary>
        /// <exception cref="ArgumentException">Returned if <paramref name="nameof(directory)"/> isn't fully qualified.</exception>
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

            // Look for the highest level existing directory or use the root
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

        /// <summary>
        /// Get the current directory for the volume of the given path. If no path is given, returns the volume for the last
        /// set current directory.
        /// </summary>
        public string GetCurrentDirectory(string path = null)
        {
            // Find the path's volume or use the last set volume if there is no path
            string volume = path == null ? _lastVolume : _extendedFileService.GetCanonicalRoot(_fileService, path);

            string directory;
            if (_volumeDirectories.TryGetValue(volume, out directory))
            {
                // We have a current directory from this volume
                AddEntry(directory, volume);
                return directory;
            }
            else
            {
                // No current directory yet for this volume

                // Try to get the hidden environment variable (e.g. "=C:") for the given drive if available
                string driveLetter = _extendedFileService.GetDriveLetter(_fileService, path);
                if (!string.IsNullOrEmpty(driveLetter))
                {
                    string environmentPath = NativeMethods.GetEnvironmentVariable("=" + driveLetter.Substring(0, 2));
                    if (environmentPath != null)
                    {
                        AddEntry(environmentPath);
                        return environmentPath;
                    }
                }

                // No stashed environment variable, add the root of the path as our current directory
                string root = Paths.GetRoot(path);
                AddEntry(root);
                return root;
            }
        }
    }
}
