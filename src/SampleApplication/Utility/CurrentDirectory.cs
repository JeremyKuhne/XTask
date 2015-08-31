// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Utility
{
    using System;
    using System.Collections.Generic;
    using XTask.Systems.File;

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
            if (Paths.IsPathRelative(directory))
            {
                throw new InvalidOperationException();
            }

            string canonicalRoot = fileService.GetCanonicalRoot(directory);
            this.lastVolume = canonicalRoot;

            if (this.volumeDirectories.ContainsKey(canonicalRoot))
            {
                this.volumeDirectories[canonicalRoot] = directory;
            }
            else
            {
                this.volumeDirectories.Add(canonicalRoot, directory);
            }
        }

        public string GetCurrentDirectory(string volumeName = null)
        {
            if (volumeName == null)
            {
                volumeName = lastVolume;
            }

            string directory;
            if (this.volumeDirectories.TryGetValue(volumeName, out directory))
            {
                return directory;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
