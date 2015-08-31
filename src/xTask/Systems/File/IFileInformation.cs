// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    /// <summary>
    /// Information about a file
    /// </summary>
    public interface IFileInformation : IFileSystemInformation
    {
        /// <summary>
        /// Length of the file, in bytes
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Directory info for the directory the file resides in
        /// </summary>
        IDirectoryInformation Directory { get; }

        /// <summary>
        /// MD5 hash of the file contents.
        /// </summary>
        byte[] MD5Hash { get; }
    }
}
