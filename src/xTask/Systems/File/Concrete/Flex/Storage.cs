// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using Windows.Support;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using xTask.Utility;
using static Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES;

namespace XTask.Systems.File.Concrete.Flex;

internal static class StorageExtensions
{
    public static uint ExtractFileAttribtues(this FILE_FLAGS_AND_ATTRIBUTES flags)
        => (uint)(flags
            & (FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM | FILE_ATTRIBUTE_DIRECTORY
                | FILE_ATTRIBUTE_ARCHIVE | FILE_ATTRIBUTE_DEVICE | FILE_ATTRIBUTE_NORMAL | FILE_ATTRIBUTE_TEMPORARY
                | FILE_ATTRIBUTE_SPARSE_FILE | FILE_ATTRIBUTE_REPARSE_POINT | FILE_ATTRIBUTE_COMPRESSED
                | FILE_ATTRIBUTE_OFFLINE | FILE_ATTRIBUTE_NOT_CONTENT_INDEXED | FILE_ATTRIBUTE_ENCRYPTED
                | FILE_ATTRIBUTE_INTEGRITY_STREAM | FILE_ATTRIBUTE_VIRTUAL | FILE_ATTRIBUTE_NO_SCRUB_DATA
                | FILE_ATTRIBUTE_EA | FILE_ATTRIBUTE_PINNED | FILE_ATTRIBUTE_UNPINNED | FILE_ATTRIBUTE_RECALL_ON_OPEN
                | FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS));

    public static uint ExtractFileFlags(this FILE_FLAGS_AND_ATTRIBUTES flags)
        => (uint)(flags
            & (FILE_FLAG_WRITE_THROUGH | FILE_FLAG_OVERLAPPED | FILE_FLAG_NO_BUFFERING | FILE_FLAG_RANDOM_ACCESS
                | FILE_FLAG_SEQUENTIAL_SCAN | FILE_FLAG_DELETE_ON_CLOSE | FILE_FLAG_BACKUP_SEMANTICS
                | FILE_FLAG_POSIX_SEMANTICS | FILE_FLAG_SESSION_AWARE | FILE_FLAG_OPEN_REPARSE_POINT
                | FILE_FLAG_OPEN_NO_RECALL | FILE_FLAG_FIRST_PIPE_INSTANCE));

    public static uint ExtractSecurityFlags(this FILE_FLAGS_AND_ATTRIBUTES flags)
        => (uint)(flags
            & (SECURITY_IDENTIFICATION | SECURITY_IMPERSONATION | SECURITY_DELEGATION | SECURITY_CONTEXT_TRACKING
                | SECURITY_EFFECTIVE_ONLY | SECURITY_SQOS_PRESENT | SECURITY_VALID_SQOS_FLAGS));

    public static FILE_ACCESS_RIGHTS ToDesiredAccess(this FileAccess fileAccess)
        => fileAccess switch
        {
            // See FileStream.Init to see how the mapping is done in .NET
            FileAccess.Read => FILE_ACCESS_RIGHTS.FILE_GENERIC_READ,
            FileAccess.Write => FILE_ACCESS_RIGHTS.FILE_GENERIC_WRITE,
            FileAccess.ReadWrite => FILE_ACCESS_RIGHTS.FILE_GENERIC_READ | FILE_ACCESS_RIGHTS.FILE_GENERIC_WRITE,
            _ => 0,
        };
}

internal unsafe static class Storage
{
    /// <summary>
    ///  Create the given directory.
    /// </summary>
    public static void CreateDirectory(string path) => Interop.CreateDirectory(path, null).ThrowLastErrorIfFalse();

    /// <summary>
    ///  Get a stream for the specified file.
    /// </summary>
    public static FileStream CreateFileStream(
        string path,
        FileAccess fileAccess,
        FileShare shareMode,
        FileMode fileMode,
        FileAttributes fileAttributes = default,
        FILE_FLAGS_AND_ATTRIBUTES flagsAndAttributes = default)
    {
        var fileHandle = CreateFile(path, fileAccess, shareMode, fileMode, fileAttributes, flagsAndAttributes);

        // FileStream will own the lifetime of the handle
        return new FileStream(
            handle: fileHandle,
            access: fileAccess,
            bufferSize: 4096,
            isAsync: flagsAndAttributes.HasFlag(FILE_FLAG_OVERLAPPED));
    }

    public static SafeFileHandle CreateFile(
        string path,
        FileAccess fileAccess,
        FileShare shareMode,
        FileMode fileMode,
        FileAttributes fileAttributes = default,
        FILE_FLAGS_AND_ATTRIBUTES flagsAndAttributes = default)
    {
        CREATEFILE2_EXTENDED_PARAMETERS extended = new()
        {
            dwSize = (uint)sizeof(CREATEFILE2_EXTENDED_PARAMETERS),
            dwFileAttributes = (uint)fileAttributes | flagsAndAttributes.ExtractFileAttribtues(),
            dwFileFlags = flagsAndAttributes.ExtractFileFlags(),
            dwSecurityQosFlags = flagsAndAttributes.ExtractSecurityFlags()
        };

        HANDLE handle = Interop.CreateFile2(
            lpFileName: path,
            dwDesiredAccess: (uint)fileAccess.ToDesiredAccess(),
            dwShareMode: (FILE_SHARE_MODE)shareMode,
            dwCreationDisposition: fileMode == FileMode.Append
                ? FILE_CREATION_DISPOSITION.OPEN_ALWAYS
                : (FILE_CREATION_DISPOSITION)fileMode,
            pCreateExParams: extended);

        if (handle == HANDLE.INVALID_HANDLE_VALUE)
        {
            Error.GetLastError().Throw(path.ToString());
        }

        return new(handle, ownsHandle: true);
    }

    /// <summary>
    ///  Tries to get file info, returns <see langword="false"/> if the given path doesn't exist.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    ///  Thrown if there aren't rights to get info for the given path.
    /// </exception>
    public static bool TryGetFileInfo(string path, out WIN32_FILE_ATTRIBUTE_DATA data)
    {
        fixed (void* d = &data)
        {
            if (Interop.GetFileAttributesEx(path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, d))
            {
                return true;
            }
        }

        WIN32_ERROR error = Error.GetLastError();
        if (error == WIN32_ERROR.ERROR_ACCESS_DENIED || error == WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED)
        {
            error.Throw(path);
        }

        return false;
    }

    /// <summary>
    ///  Gets file info.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    ///  Thrown if there aren't rights to get info for the given path.
    /// </exception>
    public static WIN32_FILE_ATTRIBUTE_DATA GetFileInfo(string path)
    {
        WIN32_FILE_ATTRIBUTE_DATA data;
        Interop.GetFileAttributesEx(path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, &data).ThrowLastErrorIfFalse(path);
        return data;
    }

    /// <summary>
    ///  Get the fully resolved path name.
    /// </summary>
    public static unsafe string GetFullPathName(string path)
    {
        using BufferScope<char> buffer = new(stackalloc char[256]);
        while (true)
        {
            fixed (char* b = buffer)
            {
                uint count = Interop.GetFullPathName(path, (uint)buffer.Length, b, null);

                if (count == 0)
                {
                    Error.ThrowLastError(path);
                }

                if (count < buffer.Length)
                {
                    return buffer.Slice(0, buffer.Length).ToString();
                }

                buffer.EnsureCapacity((int)count);
            }
        }
    }

    public static FILE_BASIC_INFO GetFileBasicInfo(SafeFileHandle fileHandle)
    {
        FILE_BASIC_INFO info = default;
        Interop.GetFileInformationByHandleEx(
            (HANDLE)fileHandle.DangerousGetHandle(),
            FILE_INFO_BY_HANDLE_CLASS.FileBasicInfo,
            &info,
            (uint)sizeof(FILE_BASIC_INFO)).ThrowLastErrorIfFalse();

        GC.KeepAlive(fileHandle);
        return info;
    }

    public static string GetFinalPathNameByHandle(
        SafeFileHandle fileHandle,
        GETFINALPATHNAMEBYHANDLE_FLAGS flags = GETFINALPATHNAMEBYHANDLE_FLAGS.FILE_NAME_NORMALIZED | GETFINALPATHNAMEBYHANDLE_FLAGS.VOLUME_NAME_DOS)
    {
        using BufferScope<char> buffer = new(stackalloc char[260]);

        HANDLE handle = (HANDLE)fileHandle.DangerousGetHandle();
        string result = null;

        while (true)
        {
            fixed (char* b = buffer)
            {
                uint count = Interop.GetFinalPathNameByHandle(
                    handle,
                    b,
                    (uint)buffer.Length,
                    flags);

                if (count == 0)
                {
                    Error.ThrowLastError();
                }

                if (count < buffer.Length)
                {
                    result = buffer.Slice(0, (int)count).ToString();
                    break;
                }

                buffer.EnsureCapacity((int)count);
            }

        }

        // Need this to be out of the loop to ensure it's always in force as it loops above.
        GC.KeepAlive(fileHandle);
        return result;
    }
}
