// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using XTask.Systems.File;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    internal static partial class NativeMethods
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364232.aspx
        internal static class FileManagement
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                internal const uint INVALID_FILE_ATTRIBUTES = unchecked((uint)(-1));
                // internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
                // internal const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;
                // internal const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;

                internal const uint GENERIC_READ = 0x80000000;
                internal const uint GENERIC_WRITE = 0x40000000;
                // internal const uint GENERIC_EXECUTE = 0x20000000;
                // internal const uint GENERIC_ALL = 0x10000000;

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364944.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern FileAttributes GetFileAttributesW(
                    string lpFileName);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365535.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool SetFileAttributesW(
                    string lpFileName,
                    uint dwFileAttributes);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364980.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetLongPathNameW(
                    string lpszShortPath,
                    SafeHandle lpszLongPath,
                    uint cchBuffer);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364989.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetShortPathNameW(
                    string lpszLongPath,
                    SafeHandle lpszShortPath,
                    uint cchBuffer);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364963.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetFullPathNameW(
                    string lpFileName,
                    uint nBufferLength,
                    SafeHandle lpBuffer,
                    IntPtr lpFilePart);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364962.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetFinalPathNameByHandleW(
                    SafeFileHandle hFile,
                    SafeHandle lpszFilePath,
                    uint cchFilePath,
                    FinalPathFlags dwFlags);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern SafeFileHandle CreateFileW(
                    string lpFileName,
                    uint dwDesiredAccess,
                    [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
                    IntPtr lpSecurityAttributes,
                    [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
                    AllFileAttributeFlags dwFlagsAndAttributes,
                    IntPtr hTemplateFile);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364419.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                internal static extern SafeFindHandle FindFirstFileExW(
                     string lpFileName,
                     FINDEX_INFO_LEVELS fInfoLevelId,
                     out WIN32_FIND_DATA lpFindFileData,
                     FINDEX_SEARCH_OPS fSearchOp,
                     IntPtr lpSearchFilter,                 // Reserved
                     int dwAdditionalFlags);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364428.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool FindNextFileW(
                    IntPtr hFindFile,
                    out WIN32_FIND_DATA lpFindFileData);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364413.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool FindClose(
                    IntPtr hFindFile);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364952.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetFileInformationByHandle(
                    SafeFileHandle hFile,
                    out BY_HANDLE_FILE_INFORMATION lpFileInformation);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363915.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool DeleteFileW(
                    string lpFilename);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365467.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                unsafe internal static extern bool ReadFile(
                    SafeFileHandle hFile,
                    byte* lpBuffer,
                    uint nNumberOfBytesToRead,
                    out uint lpNumberOfBytesRead,
                    NativeOverlapped* lpOverlapped);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363852.aspx
                // CopyFile calls CopyFileEx with COPY_FILE_FAIL_IF_EXISTS if fail if exists is set
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CopyFileExW(
                    string lpExistingFileName,
                    string lpNewFileName,
                    CopyProgressRoutine lpProgressRoutine,
                    IntPtr lpData,
                    [MarshalAs(UnmanagedType.Bool)] ref bool pbCancel,
                    CopyFileFlags dwCopyFlags);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363866.aspx
                // Note that CreateSymbolicLinkW returns a BOOLEAN (byte), not a BOOL (int)
                [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                internal static extern byte CreateSymbolicLinkW(
                    string lpSymlinkFileName,
                    string lpTargetFileName,
                    uint dwFlags);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364960.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
                internal static extern FileType GetFileType(
                    SafeFileHandle hFile);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364439.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool FlushFileBuffers(
                    SafeFileHandle hFile);
            }

            internal const FileAttributes InvalidFileAttributes = unchecked((FileAttributes)Private.INVALID_FILE_ATTRIBUTES);

            internal static FileAttributes GetFileAttributes(string path)
            {
                path = Paths.AddExtendedPrefix(path);

                FileAttributes result = Private.GetFileAttributesW(path);
                if (result == InvalidFileAttributes)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }

                return result;
            }

            private static FileAttributes TryGetFileAttributesPrivate(string path)
            {
                path = Paths.AddExtendedPrefix(path);

                FileAttributes result = Private.GetFileAttributesW(path);
                if (result == InvalidFileAttributes)
                {
                    int error = Marshal.GetLastWin32Error();
                    switch (error)
                    {
                        case WinError.ERROR_ACCESS_DENIED:
                        case WinError.ERROR_NETWORK_ACCESS_DENIED:
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture, "{0} : '{1}'", NativeErrorHelper.LastErrorToString(error), path));
                    }
                }

                return result;
            }

            /// <summary>
            /// Gets the file attributes if possible.
            /// </summary>
            /// <remarks>
            /// This is far greater perf than System.IO as it can skip all of the validation and normalization.
            /// </remarks>
            /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user does not have rights to the specified path.</exception>
            /// <returns>'false' if the path is not valid or doesn't exist.</returns>
            internal static bool TryGetFileAttributes(string path, out FileAttributes attributes)
            {
                attributes = TryGetFileAttributesPrivate(path);
                return attributes != InvalidFileAttributes;
            }

            /// <summary>
            /// Returns true if the path exists (file OR directory)
            /// </summary>
            /// <remarks>
            /// This is far greater perf than System.IO as it can skip all of the validation and normalization.
            /// </remarks>
            /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user does not have rights to the specified path.</exception>
            internal static bool PathExists(string path)
            {
                // If we didn't get invalid file attributes it must actually exist
                return TryGetFileAttributesPrivate(path) != InvalidFileAttributes;
            }

            /// <summary>
            /// Returns true if the path exists AND is a file
            /// </summary>
            /// <remarks>
            /// This is far greater perf than System.IO as it can skip all of the validation and normalization.
            /// </remarks>
            /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user does not have rights to the specified path.</exception>
            internal static bool FileExists(string path)
            {
                FileAttributes attributes = TryGetFileAttributesPrivate(path);
                if (attributes == InvalidFileAttributes)
                {
                    // Nothing there or bad path name
                    return false;
                }
                return (attributes & FileAttributes.Directory) == 0;
            }

            /// <summary>
            /// Returns true if the path exists AND is a directory
            /// </summary>
            /// <remarks>
            /// This is far greater perf than System.IO as it can skip all of the validation and normalization.
            /// </remarks>
            /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user does not have rights to the specified path.</exception>
            internal static bool DirectoryExists(string path)
            {
                FileAttributes attributes = TryGetFileAttributesPrivate(path);
                if (attributes == InvalidFileAttributes)
                {
                    // Nothing there or bad path name
                    return false;
                }
                return (attributes & FileAttributes.Directory) != 0;
            }

            internal static void SetFileAttributes(string path, FileAttributes attributes)
            {
                if (!Private.SetFileAttributesW(path, (uint)attributes))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }
            }

            /// <summary>
            /// Set the attributes on the given path if possible.
            /// </summary>
            /// <remarks>
            /// This is far greater perf than System.IO as it can skip all of the validation and normalization.
            /// </remarks>
            /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user does not have rights to the specified path.</exception>
            internal static bool TrySetFileAttributes(string path, FileAttributes attributes)
            {
                if (!Private.SetFileAttributesW(path, (uint)attributes))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    switch (lastError)
                    {
                        case WinError.ERROR_ACCESS_DENIED:
                        case WinError.ERROR_NETWORK_ACCESS_DENIED:
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture, "{0} : '{1}'", NativeErrorHelper.LastErrorToString(lastError), path));
                    }

                    return false;
                }

                return true;
            }

            internal static string GetLongPathName(string path)
            {
                return BufferPathInvoke(path, (value, buffer) => Private.GetLongPathNameW(value, buffer, buffer.CharCapacity));
            }

            internal static string GetShortPathName(string path)
            {
                return BufferPathInvoke(path, (value, buffer) => Private.GetShortPathNameW(value, buffer, buffer.CharCapacity));
            }

            /// <summary>
            /// Gets the full path name, resolving against the current working directory.  It does evaluate relative segments (".." and ".").
            /// It does not validate the path format being correct or existence of files.  Note that this does not have the normal path
            /// length limitation (MAX_PATH).
            /// </summary>
            internal static string GetFullPathName(string path)
            {
                return BufferPathInvoke(path, (value, buffer) => Private.GetFullPathNameW(value, buffer.CharCapacity, buffer, IntPtr.Zero), utilizeExtendedSyntax: false);
            }

            [Flags]
            internal enum FinalPathFlags : uint
            {
                /// <summary>
                /// Return the normalized drive name. This is the default.
                /// </summary>
                FILE_NAME_NORMALIZED = 0x0,
                /// <summary>
                /// Return the path with the drive letter. This is the default.
                /// </summary>
                VOLUME_NAME_DOS = 0x0,
                /// <summary>
                /// Return the path with a volume GUID path instead of the drive name.
                /// </summary>
                VOLUME_NAME_GUID = 0x1,
                /// <summary>
                /// Return the path with the volume device path.
                /// </summary>
                VOLUME_NAME_NT = 0x2,
                /// <summary>
                /// Return the path with no drive information.
                /// </summary>
                VOLUME_NAME_NONE = 0x4,
                /// <summary>
                /// Return the opened file name (not normalized).
                /// </summary>
                FILE_NAME_OPENED = 0x8,
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/gg258117.aspx
            [Flags]
            internal enum AllFileAttributeFlags : uint
            {
                NONE = 0x0,
                FILE_ATTRIBUTE_READONLY = 0x00000001,
                FILE_ATTRIBUTE_HIDDEN = 0x00000002,
                FILE_ATTRIBUTE_SYSTEM = 0x00000004,
                FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
                FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
                FILE_ATTRIBUTE_DEVICE = 0x00000040,
                FILE_ATTRIBUTE_NORMAL = 0x00000080,
                FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
                FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
                FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
                FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
                FILE_ATTRIBUTE_OFFLINE = 0x00001000,
                FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
                FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,
                FILE_ATTRIBUTE_INTEGRITY_STREAM = 0x00008000,
                FILE_ATTRIBUTE_VIRTUAL = 0x00010000,
                FILE_ATTRIBUTE_NO_SCRUB_DATA = 0x00020000,
                FILE_ATTRIBUTE_EA = 0x00040000,
                FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
                FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
                FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
                FILE_FLAG_SESSION_AWARE = 0x00800000,
                FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
                FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
                FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
                FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
                FILE_FLAG_RANDOM_ACCESS = 0x10000000,
                FILE_FLAG_NO_BUFFERING = 0x20000000,
                FILE_FLAG_OVERLAPPED = 0x40000000,
                FILE_FLAG_WRITE_THROUGH = 0x80000000,
                SECURITY_SQOS_PRESENT = 0x00100000,
                SECURITY_ANONYMOUS = (SecurityImpersonationLevel.SecurityAnonymous << 16),
                SECURITY_IDENTIFICATION = (SecurityImpersonationLevel.SecurityIdentification << 16),
                SECURITY_IMPERSONATION = (SecurityImpersonationLevel.SecurityImpersonation << 16),
                SECURITY_DELEGATION = (SecurityImpersonationLevel.SecurityDelegation << 16),
                SECURITY_CONTEXT_TRACKING = 0x00040000,
                SECURITY_EFFECTIVE_ONLY = 0x00080000
            }

            private enum SecurityImpersonationLevel : uint
            {
                SecurityAnonymous,
                SecurityIdentification,
                SecurityImpersonation,
                SecurityDelegation
            }

            internal static SafeFileHandle CreateFile(
                string path,
                FileAccess fileAccess,
                FileShare fileShare,
                FileMode creationDisposition,
                AllFileAttributeFlags flagsAndAttributes)
            {
                path = Paths.AddExtendedPrefix(path);
                if (creationDisposition == FileMode.Append) creationDisposition = FileMode.OpenOrCreate;

                uint dwDesiredAccess =
                    ((fileAccess & FileAccess.Read) != 0 ? Private.GENERIC_READ : 0) |
                    ((fileAccess & FileAccess.Write) != 0 ? Private.GENERIC_WRITE : 0);

                SafeFileHandle handle = Private.CreateFileW(path, dwDesiredAccess, fileShare, IntPtr.Zero, creationDisposition, flagsAndAttributes, IntPtr.Zero);
                if (handle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }

                return handle;
            }

            internal static string GetFinalPathName(SafeFileHandle fileHandle, FinalPathFlags finalPathFlags)
            {
                return BufferInvoke((buffer) => Private.GetFinalPathNameByHandleW(fileHandle, buffer, buffer.CharCapacity, finalPathFlags));
            }

            [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
            internal static string GetFinalPathName(string path, FinalPathFlags finalPathFlags, bool resolveLinks)
            {
                if (path == null) return null;
                string lookupPath = Paths.AddExtendedPrefix(path);

                // BackupSemantics is needed to get directory handles
                AllFileAttributeFlags createFileFlags = AllFileAttributeFlags.FILE_ATTRIBUTE_NORMAL | AllFileAttributeFlags.FILE_FLAG_BACKUP_SEMANTICS;
                if (!resolveLinks) createFileFlags |= AllFileAttributeFlags.FILE_FLAG_OPEN_REPARSE_POINT;

                string finalPath = null;

                using (SafeFileHandle file = CreateFile(
                    lookupPath,
                    // To look at metadata we don't need read or write access
                    0,
                    FileShare.ReadWrite,
                    FileMode.Open,
                    createFileFlags))
                {
                    if (file.IsInvalid)
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error, path);
                    }

                    finalPath = BufferInvoke((buffer) => Private.GetFinalPathNameByHandleW(file, buffer, buffer.CharCapacity, finalPathFlags), path);
                }

                // GetFinalPathNameByHandle will use the legacy drive for the volume (e.g. \\?\C:\). We may have started with \\?\Volume({GUID}) or some
                // other volume name format (C:\, etc.) we want to put it back.
                return Paths.ReplaceRoot(path, finalPath);
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364415.aspx
            private enum FINDEX_INFO_LEVELS
            {
                FindExInfoStandard = 0,
                FindExInfoBasic = 1
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364416.aspx
            private enum FINDEX_SEARCH_OPS
            {
                FindExSearchNameMatch = 0,
                FindExSearchLimitToDirectories = 1,
                FindExSearchLimitToDevices = 2
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365740.aspx
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WIN32_FIND_DATA
            {
                public FileAttributes dwFileAttributes;
                public ComTypes.FILETIME ftCreationTime;
                public ComTypes.FILETIME ftLastAccessTime;
                public ComTypes.FILETIME ftLastWriteTime;
                public uint nFileSizeHigh;
                public uint nFileSizeLow;
                public uint dwReserved0;
                public uint dwReserved1;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string cFileName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
                public string cAlternateFileName;
            }

            internal class FindResult
            {
                public string BasePath { get; private set; }
                public string FileName { get; private set; }
                public string AlternateFileName { get; private set; }
                public SafeFindHandle FindHandle { get; private set; }
                public FileAttributes Attributes { get; private set; }
                public DateTime Creation { get; private set; }
                public DateTime LastAccess { get; private set; }
                public DateTime LastWrite { get; private set; }
                public ulong Length { get; private set; }

                internal FindResult(SafeFindHandle handle, WIN32_FIND_DATA findData, string basePath)
                {
                    this.BasePath = basePath;
                    this.FindHandle = handle;
                    this.FileName = findData.cFileName;
                    this.AlternateFileName = findData.cAlternateFileName;
                    this.Attributes = (FileAttributes)findData.dwFileAttributes;

                    this.Creation = GetDateTime(findData.ftCreationTime);
                    this.LastAccess = GetDateTime(findData.ftLastAccessTime);
                    this.LastWrite = GetDateTime(findData.ftLastWriteTime);
                    this.Length = HighLowToLong(findData.nFileSizeHigh, findData.nFileSizeLow);
                }
            }

            private const int FIND_FIRST_EX_CASE_SENSITIVE = 1;
            private const int FIND_FIRST_EX_LARGE_FETCH = 2;

            /// <summary>
            /// Returns the find information.
            /// </summary>
            /// <param name="directoriesOnly">Attempts to filter to just directories where supported.</param>
            /// <param name="getAlternateName">Returns the alternate (short) file name in the FindResult.AlternateName field if it exists.</param>
            internal static FindResult FindFirstFile(
                string path,
                bool directoriesOnly = false,
                bool getAlternateName = false,
                bool returnNullIfNotFound = true)
            {
                if (Paths.EndsInDirectorySeparator(path))
                {
                    // Find first file does not like trailing separators so we'll cull it
                    //
                    // There is one weird special case. If we're passed a legacy root volume (e.g. C:\) then removing the
                    // trailing separator will make the path drive relative, leading to whatever the current directory is
                    // on that particular drive (randomness). (System32 for some odd reason in my tests.)
                    //
                    // You can't find a volume on it's own anyway, so we'll exit out in this case. For C: without a
                    // trailing slash it is legitimate to try and find whatever that matches. Note that we also don't need
                    // to bother checking the first character, as anything else there would be invalid.

                    path = Paths.RemoveTrailingSeparators(path);
                    if ((path.Length == 2 && path[1] == ':')   // C:
                        || (path.Length == 6 && path[5] == ':' && path.StartsWith(Paths.ExtendedPathPrefix))) // \\?\C:
                    {
                        if (returnNullIfNotFound) return null;
                        throw GetIoExceptionForError(WinError.ERROR_FILE_NOT_FOUND, path);
                    }

                }

                path = Paths.AddExtendedPrefix(path);

                WIN32_FIND_DATA findData;
                SafeFindHandle handle = Private.FindFirstFileExW(
                    path,
                    getAlternateName ? FINDEX_INFO_LEVELS.FindExInfoStandard : FINDEX_INFO_LEVELS.FindExInfoBasic,
                    out findData,
                    // FINDEX_SEARCH_OPS.FindExSearchNameMatch is what FindFirstFile calls Ex wtih
                    directoriesOnly ? FINDEX_SEARCH_OPS.FindExSearchLimitToDirectories :FINDEX_SEARCH_OPS.FindExSearchNameMatch, 
                    IntPtr.Zero,
                    FIND_FIRST_EX_LARGE_FETCH);

                if (handle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == WinError.ERROR_FILE_NOT_FOUND && returnNullIfNotFound)
                    {
                        return null;
                    }
                    throw GetIoExceptionForError(error, path);
                }

                return new FindResult(handle, findData, Paths.GetDirectory(path));
            }

            internal static FindResult FindNextFile(FindResult initialResult)
            {
                WIN32_FIND_DATA findData;
                if (!Private.FindNextFileW(initialResult.FindHandle.DangerousGetHandle(), out findData))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == WinError.ERROR_NO_MORE_FILES)
                    {
                        return null;
                    }
                    throw GetIoExceptionForError(error);
                }

                return new FindResult(initialResult.FindHandle, findData, initialResult.BasePath);
            }

            internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
            {
                internal SafeFindHandle() : base(true) { }

                override protected bool ReleaseHandle()
                {
                    return Private.FindClose(handle);
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363788.aspx
            [StructLayout(LayoutKind.Sequential)]
            internal struct BY_HANDLE_FILE_INFORMATION
            {
                public FileAttributes dwFileAttributes;
                public ComTypes.FILETIME ftCreationTime;
                public ComTypes.FILETIME ftLastAccessTime;
                public ComTypes.FILETIME ftLastWriteTime;
                public uint dwVolumeSerialNumber;
                public uint nFileSizeHigh;
                public uint nFileSizeLow;
                public uint nNumberOfLinks;
                public uint nFileIndexHigh;
                public uint nFileIndexLow;
            }

            internal static BY_HANDLE_FILE_INFORMATION GetFileInformationByHandle(SafeFileHandle fileHandle)
            {
                BY_HANDLE_FILE_INFORMATION fileInformation;
                if (!Private.GetFileInformationByHandle(fileHandle, out fileInformation))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }
                return fileInformation;
            }

            internal static void DeleteFile(string path)
            {
                Debug.Assert(!Paths.IsRelative(path));

                // Can't delete Posix files (end with "." for example) unless we've got the prefix
                path = Paths.AddExtendedPrefix(path, addIfUnderLegacyMaxPath: true);
                if (!Private.DeleteFileW(path))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }
            }

            /// <summary>
            /// Read the specified number of bytes synchronously. Returns the number of bytes read.
            /// </summary>
            unsafe internal static uint ReadFileSynchronous(SafeFileHandle handle, byte[] buffer, uint numberOfBytes)
            {
                uint numberOfBytesRead;

                int error = WinError.ERROR_SUCCESS;
                fixed(byte* pinnedBuffer = buffer)
                {
                    if (!Private.ReadFile(handle, pinnedBuffer, numberOfBytes, out numberOfBytesRead, null))
                    {
                        error = Marshal.GetLastWin32Error();
                    }
                }

                if (error != WinError.ERROR_SUCCESS)
                {
                    throw GetIoExceptionForError(error);
                }

                return numberOfBytesRead;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363854.aspx
            internal enum CopyProgressResult : uint
            {
                /// <summary>
                /// Continue the copy operation.
                /// </summary>
                PROGRESS_CONTINUE = 0,

                /// <summary>
                /// Cancel the copy operation and delete the destination file.
                /// </summary>
                PROGRESS_CANCEL = 1,

                /// <summary>
                /// Stop the copy operation. It can be restarted at a later time.
                /// </summary>
                PROGRESS_STOP = 2,

                /// <summary>
                /// Continue the copy operation, but stop invoking CopyProgressRoutine to report progress.
                /// </summary>
                PROGRESS_QUIET = 3
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363854.aspx
            private enum CopyProgressCallbackReason : uint
            {
                /// <summary>
                /// Another part of the data file was copied.
                /// </summary>
                CALLBACK_CHUNK_FINISHED = 0x00000000,

                /// <summary>
                /// Another stream was created and is about to be copied. This is the callback reason given when the callback routine is first invoked.
                /// </summary>
                CALLBACK_STREAM_SWITCH = 0x00000001
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363854.aspx
            private delegate CopyProgressResult CopyProgressRoutine(
                 long TotalFileSize,
                 long TotalBytesTransferred,
                 long StreamSize,
                 long StreamBytesTransferred,
                 uint dwStreamNumber,
                 CopyProgressCallbackReason dwCallbackReason,
                 IntPtr hSourceFile,
                 IntPtr hDestinationFile,
                 IntPtr lpData);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363852.aspx
            [Flags]
            internal enum CopyFileFlags : uint
            {
                /// <summary>
                /// The copy operation fails immediately if the target file already exists.
                /// </summary>
                COPY_FILE_FAIL_IF_EXISTS = 0x00000001,

                /// <summary>
                /// Progress of the copy is tracked in the target file in case the copy fails. The failed copy can be restarted at a later time by specifying the same values
                /// for lpExistingFileName and lpNewFileName as those used in the call that failed. This can significantly slow down the copy operation as the new file may
                /// be flushed multiple times during the copy operation.
                /// </summary>
                COPY_FILE_RESTARTABLE = 0x00000002,

                /// <summary>
                /// The file is copied and the original file is opened for write access.
                /// </summary>
                COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,

                /// <summary>
                /// An attempt to copy an encrypted file will succeed even if the destination copy cannot be encrypted.
                /// </summary>
                COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,

                /// <summary>
                /// If the source file is a symbolic link, the destination file is also a symbolic link pointing to the same file that the source symbolic link is pointing to. 
                /// </summary>
                COPY_FILE_COPY_SYMLINK = 0x00000800,

                /// <summary>
                /// The copy operation is performed using unbuffered I/O, bypassing system I/O cache resources. Recommended for very large file transfers.
                /// </summary>
                COPY_FILE_NO_BUFFERING = 0x00001000
            }

            internal static void CopyFile(string existingPath, string newPath, bool overwrite)
            {
                existingPath = Paths.AddExtendedPrefix(existingPath);
                newPath = Paths.AddExtendedPrefix(newPath);

                bool cancel = false;

                if (!Private.CopyFileExW(
                    existingPath,
                    newPath,
                    null,
                    IntPtr.Zero,
                    ref cancel,
                    overwrite ? 0 : CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, existingPath);
                }
            }

            private const uint SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;

            internal static void CreateSymbolicLink(string symbolicLinkPath, string targetPath, bool targetIsDirectory = false)
            {
                if (Private.CreateSymbolicLinkW(symbolicLinkPath, targetPath, targetIsDirectory ? SYMBOLIC_LINK_FLAG_DIRECTORY : 0) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, symbolicLinkPath);
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364960.aspx
            internal enum FileType : uint
            {
                FILE_TYPE_UNKNOWN = 0x0000,
                FILE_TYPE_DISK = 0x0001,
                FILE_TYPE_CHAR = 0x0002,
                FILE_TYPE_PIPE = 0x0003,
                // Unused
                // FILE_TYPE_REMOTE = 0x8000
            }

            internal static FileType GetFileType(SafeFileHandle fileHandle)
            {
                FileType fileType = Private.GetFileType(fileHandle);
                if (fileType == FileType.FILE_TYPE_UNKNOWN)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != WinError.NO_ERROR)
                    {
                        throw GetIoExceptionForError(error);
                    }
                }

                return fileType;
            }

            internal static void FlushFileBuffers(SafeFileHandle fileHandle)
            {
                if (!Private.FlushFileBuffers(fileHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }
            }
        }
    }
}
