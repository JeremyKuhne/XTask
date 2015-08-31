// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using XTask.FileSystem;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Utility;
    using ComTypes = System.Runtime.InteropServices.ComTypes;
    using System.Threading;

    internal static partial class NativeMethods
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class FileManagement
        {
            internal const uint INVALID_FILE_ATTRIBUTES = unchecked((uint)(-1));
            internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            internal const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;
            internal const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364944.aspx
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetFileAttributes")]
            private static extern uint GetFileAttributesPrivate(string lpFileName);

            internal static FileAttributes GetFileAttributes(string path)
            {
                path = Paths.AddExtendedPathPrefix(path);

                uint result = FileManagement.GetFileAttributesPrivate(path);
                if (result == FileManagement.INVALID_FILE_ATTRIBUTES)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }

                return ConvertToFileAttributes(result);
            }

            private static uint TryGetFileAttributesPrivate(string path)
            {
                path = Paths.AddExtendedPathPrefix(path);

                uint result = FileManagement.GetFileAttributesPrivate(path);
                if (result == FileManagement.INVALID_FILE_ATTRIBUTES)
                {
                    int error = Marshal.GetLastWin32Error();
                    switch (error)
                    {
                        case WinError.ERROR_ACCESS_DENIED:
                        case WinError.ERROR_NETWORK_ACCESS_DENIED:
                            throw new UnauthorizedAccessException(String.Format(CultureInfo.InvariantCulture, "{0} : '{1}'", NativeErrorHelper.LastErrorToString(error), path));
                    }
                }

                return result;
            }

            private static FileAttributes ConvertToFileAttributes(uint attributes)
            {
                // Virtual is the only attribute that isn't defined in .NET's FileAttributes- it is reserved currently
                // https://msdn.microsoft.com/en-us/library/windows/desktop/gg258117(v=vs.85).aspx
                attributes &= ~FileManagement.FILE_ATTRIBUTE_VIRTUAL;
                return (FileAttributes)attributes;
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
                attributes = 0;
                uint result = FileManagement.TryGetFileAttributesPrivate(path);
                if (result == FileManagement.INVALID_FILE_ATTRIBUTES)
                {
                    return false;
                }

                attributes = FileManagement.ConvertToFileAttributes(result);

                return true;
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
                return FileManagement.TryGetFileAttributesPrivate(path) != FileManagement.INVALID_FILE_ATTRIBUTES;
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
                uint attributes = FileManagement.TryGetFileAttributesPrivate(path);
                if (attributes == FileManagement.INVALID_FILE_ATTRIBUTES)
                {
                    // Nothing there or bad path name
                    return false;
                }
                return (attributes & FileManagement.FILE_ATTRIBUTE_DIRECTORY) != FileManagement.FILE_ATTRIBUTE_DIRECTORY;
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
                uint attributes = FileManagement.TryGetFileAttributesPrivate(path);
                if (attributes == FileManagement.INVALID_FILE_ATTRIBUTES)
                {
                    // Nothing there or bad path name
                    return false;
                }
                return (attributes & FileManagement.FILE_ATTRIBUTE_DIRECTORY) == FileManagement.FILE_ATTRIBUTE_DIRECTORY;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365535.aspx
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "SetFileAttributes")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetFileAttributesPrivate(string lpFileName, uint dwFileAttributes);

            internal static void SetFileAttributes(string path, FileAttributes attributes)
            {
                if (!FileManagement.SetFileAttributesPrivate(path, (uint)attributes))
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
                if (!FileManagement.SetFileAttributesPrivate(path, (uint)attributes))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    switch (lastError)
                    {
                        case WinError.ERROR_ACCESS_DENIED:
                        case WinError.ERROR_NETWORK_ACCESS_DENIED:
                            throw new UnauthorizedAccessException(String.Format(CultureInfo.InvariantCulture, "{0} : '{1}'", NativeErrorHelper.LastErrorToString(lastError), path));
                    }

                    return false;
                }

                return true;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364980.aspx
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetLongPathName", SetLastError = true)]
            private static extern uint GetLongPathNamePrivate(
                string lpszShortPath,
                StringBuilder lpszLongPath,
                uint cchBuffer);

            internal static string GetLongPathName(string path)
            {
                return NativeMethods.ConvertString(path, (value, sb) => FileManagement.GetLongPathNamePrivate(value, sb, (uint)sb.Capacity));
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364989.aspx
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathName", SetLastError = true)]
            private static extern uint GetShortPathNamePrivate(
                string lpszLongPath,
                StringBuilder lpszShortPath,
                uint cchBuffer);

            internal static string GetShortPathName(string path)
            {
                return NativeMethods.ConvertString(path, (value, sb) => FileManagement.GetShortPathNamePrivate(value, sb, (uint)sb.Capacity));
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364963.aspx
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetFullPathName", SetLastError = true)]
            private static extern uint GetFullPathNamePrivate(
                string lpFileName,
                uint nBufferLength,
                StringBuilder lpBuffer,
                IntPtr lpFilePart);

            /// <summary>
            /// Gets the full path name, resolving against the current working directory.  It does evaluate relative segments (".." and ".").
            /// It does not validate the path format being correct or existence of files.  Note that this does not have the normal path
            /// length limitation (MAX_PATH).
            /// </summary>
            internal static string GetFullPathName(string path)
            {
                return NativeMethods.ConvertString(path, (value, sb) => FileManagement.GetFullPathNamePrivate(value, (uint)sb.Capacity, sb, IntPtr.Zero), utilizeExtendedSyntax: false);
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

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364962.aspx
            [DllImport("Kernel32.dll", EntryPoint = "GetFinalPathNameByHandle", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern uint GetFinalPathNameByHandlePrivate(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, FinalPathFlags dwFlags);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858.aspx
            [DllImport("Kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern SafeFileHandle CreateFilePrivate(
                string fileName,
                [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
                [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
                IntPtr securityAttributes,
                [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                AllFileAttributeFlags flagsAndAttributes,
                IntPtr template);

            internal static SafeFileHandle CreateFile(
                string path,
                FileAccess fileAccess,
                FileShare fileShare,
                FileMode creationDisposition,
                AllFileAttributeFlags flagsAndAttributes)
            {
                path = Paths.AddExtendedPathPrefix(path);
                if (creationDisposition == FileMode.Append) creationDisposition = FileMode.OpenOrCreate;

                SafeFileHandle handle = CreateFilePrivate(path, fileAccess, fileShare, IntPtr.Zero, creationDisposition, flagsAndAttributes, IntPtr.Zero);
                if (handle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, path);
                }

                return handle;
            }

            internal static string GetFinalPathName(SafeFileHandle fileHandle, FinalPathFlags finalPathFlags)
            {
                return NativeMethods.ConvertString("GetFinalPathNameByHandle", (value, sb) => FileManagement.GetFinalPathNameByHandlePrivate(fileHandle, sb, (uint)sb.Capacity, finalPathFlags));
            }

            internal static string GetFinalPathName(string path, FinalPathFlags finalPathFlags)
            {
                if (path == null) return null;
                string lookupPath = Paths.AddExtendedPathPrefix(path);

                using (SafeFileHandle file = FileManagement.CreateFilePrivate(
                    lookupPath,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    IntPtr.Zero,
                    FileMode.Open,
                    // BackupSemantics is needed to get directory handles
                    AllFileAttributeFlags.FILE_ATTRIBUTE_NORMAL | AllFileAttributeFlags.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero))
                {
                    if (file.IsInvalid)
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error, path);
                    }

                    return NativeMethods.ConvertString(path, (value, sb) => FileManagement.GetFinalPathNameByHandlePrivate(file, sb, (uint)sb.Capacity, finalPathFlags));
                }
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
                public uint dwFileAttributes;
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
                public long Length { get; private set; }

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
                    this.Length = ((long)findData.nFileSizeHigh) << 32 | ((long)findData.nFileSizeLow & 0xFFFFFFFFL);
                }
            }

            private static DateTime GetDateTime(ComTypes.FILETIME fileTime)
            {
                return DateTime.FromFileTime((((long)fileTime.dwHighDateTime) << 32) + fileTime.dwLowDateTime);
            }

            private const int FIND_FIRST_EX_CASE_SENSITIVE = 1;
            private const int FIND_FIRST_EX_LARGE_FETCH = 2;

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364419.aspx
            [DllImport("kernel32.dll", EntryPoint = "FindFirstFileExW", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern SafeFindHandle FindFirstFileExPrivate(
                 string lpFileName,
                 FINDEX_INFO_LEVELS fInfoLevelId,
                 out WIN32_FIND_DATA lpFindFileData,
                 FINDEX_SEARCH_OPS fSearchOp,
                 IntPtr lpSearchFilter,                 // Reserved
                 int dwAdditionalFlags);

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
                if (Paths.PathEndsInDirectorySeparator(path))
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

                path = Paths.AddExtendedPathPrefix(path);

                WIN32_FIND_DATA findData;
                SafeFindHandle handle = FileManagement.FindFirstFileExPrivate(
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

                return new FindResult(handle, findData, Paths.GetDirectoryPathOrRoot(path));
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364428.aspx
            [DllImport("kernel32.dll", EntryPoint = "FindNextFile", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FindNextFilePrivate(
                IntPtr hFindFile,
                out WIN32_FIND_DATA lpFindFileData);

            internal static FindResult FindNextFile(FindResult initialResult)
            {
                WIN32_FIND_DATA findData;
                if (!FindNextFilePrivate(initialResult.FindHandle.DangerousGetHandle(), out findData))
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

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FindClose(
                IntPtr hFindFile);

            internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
            {
                internal SafeFindHandle() : base(true) { }

                override protected bool ReleaseHandle()
                {
                    return FileManagement.FindClose(handle);
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363915.aspx
            [DllImport("kernel32.dll", EntryPoint = "DeleteFileW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool DeleteFilePrivate(
                string lpFilename);

            internal static void DeleteFile(string path)
            {
                path = Paths.AddExtendedPathPrefix(path);
                if (!DeleteFilePrivate(path))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365467.aspx
            [DllImport("kernel32.dll", EntryPoint = "ReadFile", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            unsafe private static extern bool ReadFilePrivate(
                SafeFileHandle hFile,
                byte* lpBuffer,
                uint nNumberOfBytesToRead,
                out uint lpNumberOfBytesRead,
                NativeOverlapped* lpOverlapped);

            /// <summary>
            /// Read the specified number of bytes synchronously. Returns the number of bytes read.
            /// </summary>
            unsafe internal static uint ReadFileSynchronous(SafeFileHandle handle, byte[] buffer, uint numberOfBytes)
            {
                uint numberOfBytesRead;

                int error = WinError.ERROR_SUCCESS;
                fixed(byte* pinnedBuffer = buffer)
                {
                    if (!ReadFilePrivate(handle, pinnedBuffer, numberOfBytes, out numberOfBytesRead, null))
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

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363852.aspx
            // CopyFile calls CopyFileEx with COPY_FILE_FAIL_IF_EXISTS if fail if exists is set
            [DllImport("kernel32.dll", EntryPoint="CopyFileExW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CopyFileExPrivate(
                string lpExistingFileName,
                string lpNewFileName,
                CopyProgressRoutine lpProgressRoutine,
                IntPtr lpData,
                [MarshalAs(UnmanagedType.Bool)] ref bool pbCancel,
                CopyFileFlags dwCopyFlags);

            internal static void CopyFile(string existingPath, string newPath, bool overwrite)
            {
                existingPath = Paths.AddExtendedPathPrefix(existingPath);
                newPath = Paths.AddExtendedPathPrefix(newPath);

                bool cancel = false;

                if (!CopyFileExPrivate(
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

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363866(v=vs.85).aspx
            [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CreateSymbolicLinkPrivate(
                string lpSymlinkFileName,
                string lpTargetFileName,
                uint  dwFlags
            );

            private const uint SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;

            internal static void CreateSymbolicLink(string symbolicLinkPath, string targetPath, bool targetIsDirectory = false)
            {
                if (!CreateSymbolicLinkPrivate(symbolicLinkPath, targetPath, targetIsDirectory ? SYMBOLIC_LINK_FLAG_DIRECTORY : 0))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, symbolicLinkPath);
                }
            }
        }
    }
}
