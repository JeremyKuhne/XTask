// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    using System;
    using System.IO;

    public static class FileServiceExtensions
    {
        /// <summary>
        /// Returns true if the given path is readonly.
        /// </summary>
        public static bool IsReadOnly(this IFileService fileService, string path)
        {
            return fileService.HasAttributes(path, FileAttributes.ReadOnly);
        }

        /// <summary>
        /// Returns true if the given path has the specified attribute(s).
        /// </summary>
        public static bool HasAttributes(this IFileService fileService, string path, FileAttributes attributes)
        {
            return (fileService.GetAttributes(path) & attributes) != 0;
        }

        /// <summary>
        /// Returns true if the given path does not have the specified attribute(s).
        /// </summary>
        public static bool DoesNotHaveAttributes(this IFileService fileService, string path, FileAttributes attributes)
        {
            return (fileService.GetAttributes(path) & attributes) == 0;
        }

        /// <summary>
        /// Attempts to clear the specified attribute(s) on the given path.
        /// </summary>
        public static void ClearAttributes(this IFileService fileService, string path, FileAttributes attributes)
        {
            FileAttributes currentAttributes = fileService.GetAttributes(path);
            if ((currentAttributes & attributes) != 0)
            {
                fileService.SetAttributes(path, currentAttributes &= ~attributes);
            }
        }

        /// <summary>
        /// Attempts to make the given path writable if necessary.
        /// </summary>
        public static void MakeWritable(this IFileService fileService, string path)
        {
            fileService.ClearAttributes(path, FileAttributes.ReadOnly);
        }

        /// <summary>
        /// True if the given path exists and is a file.
        /// </summary>
        /// <remarks>
        /// Will only throw for unauthorized access, assumes bad paths don't exist.
        /// </remarks>
        public static bool FileExists(this IFileService fileService, string path)
        {
            try
            {
                return fileService.DoesNotHaveAttributes(path, FileAttributes.Directory);
            }
            catch (Exception e) when (!(e is UnauthorizedAccessException))
            {
                return false;
            }
        }

        /// <summary>
        /// True if the given path exists and is a directory.
        /// </summary>
        /// <remarks>
        /// Will only throw for unauthorized access, assumes bad paths don't exist.
        /// </remarks>
        public static bool DirectoryExists(this IFileService fileService, string path)
        {
            try
            {
                return fileService.HasAttributes(path, FileAttributes.Directory);
            }
            catch (Exception e) when (!(e is UnauthorizedAccessException))
            {
                return false;
            }
        }

        /// <summary>
        /// True if the given path exists. Can be a file or a directory.
        /// </summary>
        /// <remarks>
        /// Will only throw for unauthorized access, assumes bad paths don't exist.
        /// </remarks>
        public static bool PathExists(this IFileService fileService, string path)
        {
            try
            {
                fileService.GetAttributes(path);
                return true;
            }
            catch (Exception e) when (!(e is UnauthorizedAccessException))
            {
                return false;
            }
        }

        /// <summary>
        /// Gets file info for the specified path.
        /// </summary>
        /// <exception cref="FileExistsException">Thrown if a directory exists in the given path.</exception>
        public static IFileInformation GetFileInfo(this IFileService fileService, string path)
        {
            IFileInformation fileInfo = fileService.GetPathInfo(path) as IFileInformation;
            if (fileInfo == null)
            {
                throw new FileExistsException(XTaskStrings.ErrorNotAFile, path);
            }
            return fileInfo;
        }

        /// <summary>
        /// Gets directory info for the specified path.
        /// </summary>
        /// <exception cref="FileExistsException">Thrown if a file exists in the given path.</exception>
        public static IDirectoryInformation GetDirectoryInfo(this IFileService fileService, string path)
        {
            IDirectoryInformation directoryInformation = fileService.GetPathInfo(path) as IDirectoryInformation;
            if (directoryInformation == null)
            {
                throw new FileExistsException(XTaskStrings.ErrorNotADirectory, path);
            }
            return directoryInformation;
        }

        /// <summary>
        /// Simple helper to create a reader on an existing file. Dispose the reader when finished.
        /// </summary>
        public static TextReader CreateReader(this IFileService fileService, string path)
        {
            return new StreamReader(fileService.CreateFileStream(path));
        }

        /// <summary>
        /// Simple helper to create a writer on an existing file. Dispose the writer when finished.
        /// </summary>
        public static TextWriter CreateWriter(this IFileService fileService, string path)
        {
            return new StreamWriter(fileService.CreateFileStream(path, FileMode.Append, FileAccess.Write));
        }

        /// <summary>
        /// Write all of the text
        /// </summary>
        public static void WriteAllText(this IFileService fileService, string path, string text)
        {
            using (var writer = fileService.CreateWriter(path))
            {
                writer.WriteLine(text);
            }
        }
    }
}
