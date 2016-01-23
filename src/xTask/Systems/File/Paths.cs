// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Utility;
    using Interop;

    /// <summary>
    /// Path related helpers.
    /// </summary>
    /// <remarks>
    /// Code in here should NOT touch actual IO.
    /// </remarks>
    public static class Paths
    {
        /// <summary>
        /// Legacy maximum path length in Windows (without using extended syntax).
        /// </summary>
        public const int MaxPath = 260;

        /// <summary>
        /// Maximum path size using extended syntax or path APIs in the FlexFileService (default).
        /// </summary>
        /// <remarks>
        /// Windows APIs need extended syntax to get past 260 characters (including the null terminator).
        /// </remarks>
        public const int MaxLongPath = short.MaxValue;

        /// <summary>
        /// Path prefix for extended paths
        /// </summary>
        public const string ExtendedPathPrefix = @"\\?\";

        /// <summary>
        /// Path prefix for extended UNC paths
        /// </summary>
        public const string ExtendedUncPrefix = @"\\?\UNC\";

        /// <summary>
        /// Path prefix for UNC paths.
        /// </summary>
        public const string UncPrefix = @"\\";

        // - Paths are case insensitive (NTFS supports sensitivity, but it is not enabled by default)
        // - Backslash is the "correct" separator for path components. Windows APIs convert forward slashes to backslashes, except for "\\?\"
        //
        // References
        // ==========
        //
        // "Naming Files, Paths, and Namespaces"
        // http://msdn.microsoft.com/en-us/library/windows/desktop/aa365247.aspx
        //
        private static readonly char[] directorySeparatorCharacters = new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };

        /// <summary>
        /// The default directory separator
        /// </summary>
        public static readonly char DirectorySeparator = System.IO.Path.DirectorySeparatorChar;

        /// <summary>
        /// The alternate directory separator
        /// </summary>
        public static readonly char AltDirectorySeparator = System.IO.Path.AltDirectorySeparatorChar;

        /// <summary>
        /// Volume separator character
        /// </summary>
        public static readonly char VolumeSeparator = System.IO.Path.VolumeSeparatorChar;

        /// <summary>
        /// Returns true if the path specified is relative to the current drive or working directory.
        /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// </summary>
        /// <remarks>
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
        public static bool IsRelative(string path)
        {
            if (path == null) { throw new ArgumentNullException("path"); }
            if (path.Length < 2)
            {
                // It isn't fixed, it must be relative.  There is no way to specify a fixed
                // path with one character (or less).
                return true;
            }

            if ((path[0] == '\\') || (path[0] == '/'))
            {
                // There is no valid way to specify a relative path with two initial slashes
                return !((path[1] == '\\') || (path[1] == '/'));
            }

            // The only way to specify a fixed path that doesn't begin with two slashes
            // is the drive, colon, slash format- i.e. C:\
            return !((path.Length >= 3)
                && (path[1] == ':')
                && ((path[2] == '\\') || (path[2] == '/')));
        }

        /// <summary>
        /// Returns true if the given path has any of the specified extensions
        /// </summary>
        public static bool HasExtension(string path, params string[] extensions)
        {
            string pathExtension = Paths.GetExtension(path);
            return extensions.Any(extension => string.Equals(pathExtension, extension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Attempt to retreive a file extension (with period), if any, from the given path or file name. Does not throw.
        /// </summary>
        public static string GetExtension(string pathOrFileName)
        {
            int extensionIndex = Paths.FindExtensionOffset(pathOrFileName);
            if (extensionIndex == -1)
            {
                // Nothing valid- return nothing
                return String.Empty;
            }
            else
            {
                return pathOrFileName.Substring(extensionIndex);
            }
        }

        /// <summary>
        /// Returns the index of the extension for the given path.  Does not validate paths in any way.
        /// </summary>
        /// <returns>The index of the period</returns>
        private static int FindExtensionOffset(string pathOrFileName)
        {
            if (String.IsNullOrEmpty(pathOrFileName)) { return -1; }

            int length = pathOrFileName.Length;

            // If are only one character long or we end with a period, return
            if ((length == 1) || pathOrFileName[length - 1] == '.')
            {
                return -1;
            }

            // Walk the string backwards looking for a period
            int index = length;
            while (--index >= 0)
            {
                char ch = pathOrFileName[index];
                if (ch == '.')
                {
                    return index;
                }

                if (((ch == DirectorySeparator) || ch == ' ' || (ch == AltDirectorySeparator)) || (ch == VolumeSeparator))
                {
                    // Found a space, directory or volume separator before a period
                    // (this is something .NET gets wrong- extensions cannot have spaces in them)
                    return -1;
                }
            }

            // No period at all
            return -1;
        }

        /// <summary>
        /// Returns the path up to the last directory separator or the root if already at the root (e.g. "C:\", "\\Server\Share\", etc.).
        /// </summary>
        /// <returns>The directory path / root or null if the path is unknown.</returns>
        public static string GetDirectory(string path)
        {
            int directoryLength = GetDirectoryOrRootLength(path);
            if (directoryLength < 0) return null;

            path = path.Substring(0, directoryLength);
            return Paths.AddTrailingSeparator(path);
        }

        private static int GetDirectoryOrRootLength(string path, bool skipTrailingSlash = false)
        {
            int rootLength;
            PathFormat pathFormat = Paths.GetPathFormat(path, out rootLength);
            if (pathFormat == PathFormat.UnknownFormat) return -1;

            int length = path.Length;
            if (rootLength == path.Length) return length;
            if (skipTrailingSlash && EndsInDirectorySeparator(path)) length--;

            while (((length > rootLength)
                && (path[--length] != DirectorySeparator))
                && (path[length] != AltDirectorySeparator))
            {
            }

            return length;
        }

        /// <summary>
        /// Returns the file or directory name for the given path or null if already at the root or the path is unknown.
        /// </summary>
        public static string GetFileOrDirectoryName(string path)
        {
            int directoryLength = GetDirectoryOrRootLength(path, skipTrailingSlash: true);
            if (directoryLength < 0) return null;

            // Just a root? Return it.
            if (directoryLength == path.Length) return null;
            if (IsDirectorySeparator(path[directoryLength])) directoryLength++;
            return EndsInDirectorySeparator(path) ? path.Substring(directoryLength, path.Length - directoryLength - 1) : path.Substring(directoryLength);
        }

        /// <summary>
        /// Finds the topmost directories for the specified paths that contain the paths passed in.
        /// </summary>
        public static IEnumerable<string> FindCommonRoots(IEnumerable<string> paths)
        {
            HashSet<string> roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (paths == null) { return roots; }

            foreach (string path in paths)
            {
                if (String.IsNullOrWhiteSpace(path)) continue;

                string directory = Paths.GetDirectory(path);
                if (!roots.Contains(directory))
                {
                    // Remove any directories that start with this directory
                    if (roots.RemoveWhere(existingDirectory => existingDirectory.StartsWith(directory, StringComparison.OrdinalIgnoreCase)) > 0)
                    {
                        // This is shorter than others that already exist, just add it
                        //
                        // (If we find C:\Foo\Bar\Bar for C:\Foo\ and we already haven't added C:\Foo\
                        // we can't have C:\ as this and the else statement pass below would have prevented
                        // this state.)
                        roots.Add(directory);
                    }
                    else
                    {
                        // No matches, so we need to add if there isn't already a shorter path for this one
                        if (!roots.Any(root => directory.StartsWith(root, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Nothing starts our current directory, add it
                            roots.Add(directory);
                        }
                    }
                }
            }

            return roots;
        }

        /// <summary>
        /// Returns the root for the given path or null if the path format can't be determined.
        /// </summary>
        public static string GetRoot(string path)
        {
            int rootLength;
            GetPathFormat(path, out rootLength);
            if (rootLength < 0) return null;
            else return path.Substring(0, rootLength);
        }

        /// <summary>
        /// Returns the length of the root for the given path or -1 if the path format can't be determined.
        /// </summary>
        public static int GetRootLength(string path)
        {
            int rootLength;
            GetPathFormat(path, out rootLength);
            return rootLength;
        }

        /// <summary>
        /// Puts the root from the source path onto the target path if needed.
        /// If either root can't be determined just returns the target path.
        /// </summary>
        public static string ReplaceRoot(string sourcePath, string targetPath)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (targetPath == null) throw new ArgumentNullException(nameof(targetPath));

            int sourceRoot = GetRootLength(sourcePath);
            int targetRoot;

            // Skip out if we can't figure out the roots or are already good
            if (sourceRoot == -1
                || Strings.StartsWithCount(sourcePath, targetPath, sourceRoot, StringComparison.OrdinalIgnoreCase)
                || (targetRoot = GetRootLength(targetPath)) == -1)
                return targetPath;

            var sb = StringBuilderCache.Instance.Acquire();

            // Try and keep the casing of the target path
            int keepLength = Strings.FindRightmostCommonCount(sourcePath, sourceRoot -1, targetPath, targetRoot - 1, StringComparison.OrdinalIgnoreCase);

            sb.Append(sourcePath, 0, sourceRoot - keepLength);
            sb.Append(targetPath, targetRoot - keepLength, targetPath.Length - targetRoot + keepLength);
            return StringBuilderCache.Instance.ToStringAndRelease(sb);
        }

        /// <summary>
        /// Gets the format of the specified path.
        /// </summary>
        /// <remarks>
        /// Does not look for invalid characters beyond what makes for an indeterminate path.
        /// </remarks>
        public static PathFormat GetPathFormat(string path)
        {
            int rootLength;
            return GetPathFormat(path, out rootLength);
        }

        /// <summary>
        /// Gets the format and root length of the specified path. Returns -1 for the root length
        /// if the path format can't be determined.
        /// </summary>
        /// <remarks>
        /// Does not look for invalid characters beyond what makes for an indeterminate path.
        /// </remarks>
        public unsafe static PathFormat GetPathFormat(string path, out int rootLength)
        {
            if (path == null || path.Length == 0 || path[0] == ':')
            {
                rootLength = -1;
                return PathFormat.UnknownFormat;
            }

            fixed (char* start = path)
            {
                return Paths.GetPathFormat(start, path.Length, out rootLength);
            }
        }

        private static unsafe PathFormat GetPathFormat(char* path, int pathLength, out int rootLength)
        {
            rootLength = -1;

            // Forward slashes are normalized to backslashes, so consider them equivalent
            if (!(path[0] == '\\' || path[0] == '/'))
            {
                // Path does not start with a slash
                if (pathLength < 2 || path[1] != ':')
                {
                    // Just a single character, or no colon
                    rootLength = 0;
                    return PathFormat.CurrentDirectoryRelative;
                }

                // We've got a colon in the drive letter position, check for a valid drive letter
                char drive = Char.ToUpperInvariant(path[0]);
                if (!(drive >= 'A' && drive <= 'Z'))
                {
                    // Not a valid drive identifier
                    return PathFormat.UnknownFormat;
                }

                if (pathLength > 2 && (path[2] == '\\' || path[2] == '/'))
                {
                    // C:\, D:\, etc
                    rootLength = 3;
                    return PathFormat.DriveAbsolute;
                }

                rootLength = 2;
                return PathFormat.DriveRelative;
            }

            // Now we know we have a slash, a single one is current volume (drive) relative
            if (pathLength == 1 || !(path[1] == '\\') || (path[1] == '/'))
            {
                rootLength = 1;
                return PathFormat.CurrentVolumeRelative;
            }

            if (pathLength < 5 || path[2] == '\\' || path[2] == '/')
            {
                // Can't just be two or three slashes, and must be at least 5 characters \\a\b \\?\a
                return PathFormat.UnknownFormat;
            }

            // Now we know we're special (\\?\, \\.\, \\Server\Share)
            int uncRoot = 3;
            PathFormat format = PathFormat.UniformNamingConvention;
            if (path[3] == '\\' || path[3] == '/')
            {
                // If we have \\ \ of some sort we might be a  special format
                switch (path[2])
                {
                    case '.':
                        format = PathFormat.Device;
                        break;
                    case '?':
                        // Check for \\?\UNC or \\?\UNC\
                        if (path[4] == 'U' && path[5] == 'N' && path[6] == 'C'
                            && (pathLength == 7 || path[7] == '\\' || path[7] == '/'))
                        {
                            // Can't be anything but a bad or good extended UNC
                            format = PathFormat.UniformNamingConventionExtended;
                            uncRoot = 9;
                        }
                        else
                        {
                            format = PathFormat.VolumeAbsoluteExtended;
                        }
                        break;
                }

                if (format == PathFormat.Device || format == PathFormat.VolumeAbsoluteExtended)
                {
                    // At least \\?\ or \\.\, can't have another slash
                    if (path[4] == '\\' || path[4] == '/') return PathFormat.UnknownFormat;

                    // Find the end of the volume/device identifier
                    int nextSeparator = NextSeparator(path, pathLength, 4);
                    rootLength = nextSeparator > -1 ? nextSeparator + 1 : pathLength;
                    return format;
                }
            }

            // UNC root is known to be \\ (two characters) or \\?\UNC (seven characters)
            if (pathLength >= uncRoot + 2      // At least \\a\b or \\?\UNC\a\b
                && path[uncRoot - 1] != '\\')  // Not just \\\ or \\?\UNC\\
            {
                int indexOfShareSeparator = NextSeparator(path, pathLength, uncRoot);
                if (indexOfShareSeparator > -1                     // Needs at least one slash past \\?\UNC\a
                    && indexOfShareSeparator != pathLength - 1     //  and it can't be the final (e.g. \\?\UNC\a\)
                    && path[indexOfShareSeparator + 1] != '\\')    //  and it can't be two backslashes (e.g. \\?\UNC\\)
                {
                    // We're good, find the end of the server\share
                    int nextSeparator = NextSeparator(path, pathLength, indexOfShareSeparator + 1);
                    rootLength = nextSeparator > -1 ? nextSeparator + 1 : pathLength;
                    return format;
                }
            };

            // Bad extended UNC
            return PathFormat.UnknownFormat;
        }

        private unsafe static int NextSeparator(char* value, int length, int skip)
        {
            for (int i = skip; i < length; i++)
            {
                if (value[i] == DirectorySeparator || value[i] == AltDirectorySeparator) return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns true if the path begins with a directory separator.
        /// </summary>
        public static bool BeginsWithDirectorySeparator(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            return Paths.IsDirectorySeparator(path[0]);
        }

        /// <summary>
        /// Returns true if the path begins with a directory separator.
        /// </summary>
        public static bool BeginsWithDirectorySeparator(StringBuilder path)
        {
            if (path == null || path.Length == 0)
            {
                return false;
            }

            return Paths.IsDirectorySeparator(path[0]);
        }

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        public static bool EndsInDirectorySeparator(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            char lastChar = path[path.Length - 1];
            return Paths.IsDirectorySeparator(lastChar);
        }

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        public static bool EndsInDirectorySeparator(StringBuilder path)
        {
            if (path == null || path.Length == 0)
            {
                return false;
            }

            char lastChar = path[path.Length - 1];
            return Paths.IsDirectorySeparator(lastChar);
        }

        /// <summary>
        /// Returns true if the given character is a directory separator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirectorySeparator(char character)
        {
            return (character == DirectorySeparator || character == AltDirectorySeparator);
        }

        /// <summary>
        /// Ensures that the specified path ends in a directory separator.
        /// </summary>
        /// <returns>The path with an appended directory separator if necessary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if path is null.</exception>
        public static string AddTrailingSeparator(string path)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (Paths.EndsInDirectorySeparator(path))
            {
                return path;
            }
            else
            {
                return path + DirectorySeparator;
            }
        }

        /// <summary>
        /// Ensures that the specified path does not end in a directory separator.
        /// </summary>
        /// <returns>The path with an appended directory separator if necessary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if path is null.</exception>
        public static string RemoveTrailingSeparators(string path)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (Paths.EndsInDirectorySeparator(path))
            {
                return path.TrimEnd(Paths.directorySeparatorCharacters);
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// Returns true if the given path is extended.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtended(string path)
        {
            return path != null && path.StartsWith(Paths.ExtendedPathPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds the extended path prefix (\\?\) if not already present.
        /// </summary>
        /// <param name="addIfUnderLegacyMaxPath">If false, will not add the extended prefix unless needed.</param>
        public static string AddExtendedPrefix(string path, bool addIfUnderLegacyMaxPath = false)
        {
            if (IsExtended(path)
                || (!addIfUnderLegacyMaxPath && path.Length <= Paths.MaxPath))
            {
                return path;
            }

            if (!path.StartsWith(Paths.UncPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Paths.ExtendedPathPrefix + path;
            }

            // Given \\server\share in longpath becomes \\?\UNC\server\share
            var sb = StringBuilderCache.Instance.Acquire();

            // Ensure we have enough length for "\\?\UNC\" (we already have "\\")
            sb.EnsureCapacity(path.Length + 6);
            sb.Append(ExtendedUncPrefix);
            sb.Append(path, 2, path.Length - 2);
            return StringBuilderCache.Instance.ToStringAndRelease(sb);
        }

        /// <summary>
        /// Combines two strings, adding a directory separator between if needed.
        /// Does not validate path characters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path1"/> is null.</exception>
        public static string Combine(string path1, string path2)
        {
            if (path1 == null) throw new ArgumentNullException(nameof(path1));

            // Add nothing to something is something
            if (String.IsNullOrEmpty(path2)) return path1;

            StringBuilder sb = StringBuilderCache.Instance.Acquire();
            if (!EndsInDirectorySeparator(path1) && !BeginsWithDirectorySeparator(path2))
            {
                sb.Append(path1);
                sb.Append(DirectorySeparator);
                sb.Append(path2);
            }
            else
            {
                sb.Append(path1);
                sb.Append(path2);
            }

            return StringBuilderCache.Instance.ToStringAndRelease(sb);
        }

        /// <summary>
        /// Combines two string builders into the first string builder, adding a directory separator between if needed.
        /// Does not validate path characters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path1"/> is null.</exception>
        public static void Combine(StringBuilder path1, string path2)
        {
            if (path1 == null) throw new ArgumentNullException(nameof(path1));

            // Add nothing to something is something
            if (path2 == null || path2.Length == 0) return;

            if (!EndsInDirectorySeparator(path1) && !BeginsWithDirectorySeparator(path2))
            {
                path1.Append(DirectorySeparator);
                path1.Append(path2);
            }
            else
            {
                path1.Append(path2);
            }
        }

        /// <summary>
        /// Normalize the directory separators in the given path. Makes alternate directory separators into default separators and
        /// collapses runs of separators. Will keep initial two separators as these have special meaning (UNC or extended path).
        /// Does not collpase relative segments.
        /// </summary>
        public static string NormalizeDirectorySeparators(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var normalized = InternalNormalizeDirectorySeparators(path);
            if (normalized == null)
                return path;
            else
                return StringBufferCache.Instance.ToStringAndRelease(normalized);
        }


        private static StringBuffer InternalNormalizeDirectorySeparators(string path)
        {
            // Check to see if we need to normalize
            bool normalized = true;
            char current;

            for (int i = 0; i < path.Length; i++)
            {
                current = path[i];

                // If we have a separator
                if (Paths.IsDirectorySeparator(current))
                {
                    if (
                        // And it isn't the default
                        current != Paths.DirectorySeparator
                        // or it isn't the first char and the next is also a separator (to allow for UNC & extended syntax which begin with \\)
                        || (i > 0 && i < path.Length - 1 && Paths.IsDirectorySeparator(path[i + 1])))
                    {
                        normalized = false;
                        break;
                    }
                }
            }

            // Already normalized, don't allocate another string
            if (normalized) return null;

            // Normalize
            var builder = StringBufferCache.Instance.Acquire((ulong)path.Length);

            // Keep an initial separator if we start with separators
            int startSeparators = 0;
            while (startSeparators < path.Length && Paths.IsDirectorySeparator(path[startSeparators])) startSeparators++;
            if (startSeparators > 0) builder.Append(Paths.DirectorySeparator);

            // This is a special case- we want to keep *two* if we have *just* two to allow for UNCs and extended paths
            if (startSeparators == 2) builder.Append(Paths.DirectorySeparator);

            for (int i = startSeparators; i < path.Length; i++)
            {
                current = path[i];

                // If we have a separator
                if (Paths.IsDirectorySeparator(current))
                {
                    // If the next is a separator, skip adding this
                    if (i < path.Length - 1 && Paths.IsDirectorySeparator(path[i + 1]))
                    {
                        continue;
                    }

                    // Ensure it is the primary separator
                    current = Paths.DirectorySeparator;
                }

                builder.Append(current);
            }

            return builder;
        }
    }
}