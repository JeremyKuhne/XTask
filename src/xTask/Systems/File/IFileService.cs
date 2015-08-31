// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Proxy for file system access
    /// </summary>
    /// <remarks>
    /// This interface is supposed to be close to the lowest level functionality to facilitate
    /// developing impementations and sharing more complicated logic (through extension methods).
    /// </remarks>
    public interface IFileService
    {
        /// <summary>
        /// Get information for the given path, if it exists.
        /// </summary>
        /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user does not have rights to the specified path.</exception>
        /// <returns>Null if the given path doesn't exist.</returns>
        IFileSystemInformation GetPathInfo(string path);

        // IEnumerable<IFileSystemInformation> EnumerateFiles(string path, string searchPattern = "*.*");

        /// <summary>
        /// Get a stream for given path.
        /// </summary>
        Stream CreateFileStream(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite);

        /// <summary>
        /// Create a directory.
        /// </summary>
        /// <remarks>
        /// Creates intermediate directories if needed.
        /// </remarks>
        void CreateDirectory(string path);

        /// <summary>
        /// Delete the given file.
        /// </summary>
        void DeleteFile(string path);

        /// <summary>
        /// Delete the given directory.
        /// </summary>
        /// <param name="deleteChildren">Will attempt to delete all children.</param>
        void DeleteDirectory(string path, bool deleteChildren = false);

        /// <summary>
        /// Copy the specified file to the specified location.
        /// </summary>
        void CopyFile(string existingPath, string newPath, bool overwrite = false);

        /// <summary>
        /// Normalize the given path, analyzing ".." and "." segments.
        /// </summary>
        /// <param name="basePath">Optional. The path to resolve against if the path is relative.</param>
        string GetFullPath(string path, string basePath = null);

        /// <summary>
        /// Gets the attributes for the given path.
        /// </summary>
        FileAttributes GetAttributes(string path);

        /// <summary>
        /// Gets the attributes for the given path.
        /// </summary>
        void SetAttributes(string path, FileAttributes attributes);
    }
}
