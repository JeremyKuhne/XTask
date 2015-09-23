// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using XTask.Systems.File;

    internal static partial class NativeMethods
    {
        internal static class Backup
        {
            // [MS-BKUP]: Microsoft NT Backup File Structure
            // https://msdn.microsoft.com/en-us/library/dd305136.aspx
            // Most defines from WinBase.h

            /// <summary>
            /// The types returned in WIN32_STREAM_ID dwStreamId
            /// </summary>
            private enum BackupStreamType : uint
            {
                BACKUP_INVALID = 0x00000000,

                /// <summary>
                /// Standard data
                /// </summary>
                BACKUP_DATA = 0x00000001,

                /// <summary>
                /// Extended attribute data
                /// </summary>
                BACKUP_EA_DATA = 0x00000002,

                /// <summary>
                /// Security descriptor data
                /// </summary>
                BACKUP_SECURITY_DATA = 0x00000003,

                /// <summary>
                /// Alternative data streams
                /// </summary>
                BACKUP_ALTERNATE_DATA = 0x00000004,

                /// <summary>
                /// Hard link information
                /// </summary>
                BACKUP_LINK = 0x00000005,

                BACKUP_PROPERTY_DATA = 6,

                /// <summary>
                /// Object identifiers
                /// </summary>
                BACKUP_OBJECT_ID = 0x00000007,

                /// <summary>
                /// Reparse points
                /// </summary>
                BACKUP_REPARSE_DATA = 0x00000008,

                /// <summary>
                /// Data in a sparse file
                /// </summary>
                BACKUP_SPARSE_BLOCK = 0x00000009,

                /// <summary>
                /// Transactional file system
                /// </summary>
                BACKUP_TXFS_DATA = 0x0000000A
            }

            private struct StreamInfo
            {
                public string Name;
                public BackupStreamType Type;
                public ulong Size;
            }

            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private class BackupReader : IDisposable
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa362509.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                private static extern bool BackupRead(
                    SafeFileHandle hFile,
                    IntPtr lpBuffer,
                    uint nNumberOfBytesToRead,
                    out uint lpNumberOfBytesRead,
                    [MarshalAs(UnmanagedType.Bool)] bool bAbort,
                    [MarshalAs(UnmanagedType.Bool)] bool bProcessSecurity,
                    ref IntPtr context);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa362510.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                private static extern bool BackupSeek(
                    SafeFileHandle hFile,
                    uint dwLowBytesToSeek,
                    uint dwHighBytesToSeek,
                    out uint lpdwLowByteSeeked,
                    out uint lpdwHighByteSeeked,
                    ref IntPtr context);

                // https://msdn.microsoft.com/en-us/library/dd303907.aspx
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa362667.aspx
                //
                // typedef struct _WIN32_STREAM_ID
                // {
                //     DWORD dwStreamId;
                //     DWORD dwStreamAttributes;
                //     LARGE_INTEGER Size;
                //     DWORD dwStreamNameSize;
                //     WCHAR cStreamName[ANYSIZE_ARRAY];
                // }
                // WIN32_STREAM_ID, *LPWIN32_STREAM_ID;

                [StructLayout(LayoutKind.Sequential, Pack = 4)]
                private struct WIN32_STREAM_ID
                {
                    public BackupStreamType dwStreamId;
                    public BackupStreamAttributes dwStreamAttributes;
                    public ulong Size;
                    public uint dwStreamNameSize;
                }

                /// <summary>
                /// The attributes returned in WIN32_STREAM_ID dwStreamAttributes
                /// </summary>
                [Flags]
                private enum BackupStreamAttributes : uint
                {
                    /// <summary>
                    /// This backup stream has no special attributes.
                    /// </summary>
                    STREAM_NORMAL_ATTRIBUTE = 0x00000000,

                    STREAM_MODIFIED_WHEN_READ = 0x00000001,

                    /// <summary>
                    /// The backup stream contains security information. This attribute applies only to backup stream of type SECURITY_DATA.
                    /// </summary>
                    STREAM_CONTAINS_SECURITY = 0x00000002,

                    STREAM_CONTAINS_PROPERTIES = 0x00000004,

                    /// <summary>
                    /// The backup stream is part of a sparse file stream. This attribute applies only to backup stream of type DATA, ALTERNATE_DATA, and SPARSE_BLOCK.
                    /// </summary>
                    STREAM_SPARSE_ATTRIBUTE = 0x00000008
                }

                private IntPtr context = IntPtr.Zero;
                private SafeFileHandle fileHandle;
                NativeBuffer buffer = new NativeBuffer(4096);
                uint structureSize = (uint)Marshal.SizeOf(typeof(WIN32_STREAM_ID));

                public BackupReader(SafeFileHandle fileHandle)
                {
                    this.fileHandle = fileHandle;
                }

                public StreamInfo? GetNextInfo()
                {
                    uint bytesRead;
                    if (!BackupRead(
                        hFile: fileHandle,
                        lpBuffer: buffer,
                        nNumberOfBytesToRead: structureSize,
                        lpNumberOfBytesRead: out bytesRead,
                        bAbort: false,
                        bProcessSecurity: true,
                        context: ref this.context))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error);
                    }

                    // Exit if at the end
                    if (bytesRead == 0) return null;

                    WIN32_STREAM_ID streamId = (WIN32_STREAM_ID)Marshal.PtrToStructure(buffer, typeof(WIN32_STREAM_ID));
                    string name = null;
                    if (streamId.dwStreamNameSize > 0)
                    {
                        buffer.EnsureLength(streamId.dwStreamNameSize);
                        if (!BackupRead(
                            hFile: fileHandle,
                            lpBuffer: buffer,
                            nNumberOfBytesToRead: streamId.dwStreamNameSize,
                            lpNumberOfBytesRead: out bytesRead,
                            bAbort: false,
                            bProcessSecurity: true,
                            context: ref this.context))
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw GetIoExceptionForError(error);
                        }
                        name = Marshal.PtrToStringUni(buffer, (int)bytesRead / 2);
                    }

                    if (streamId.Size > 0)
                    {
                        // Move to the next header, if any
                        uint low, high;
                        if (!BackupSeek(
                            hFile: fileHandle,
                            dwLowBytesToSeek: uint.MaxValue,
                            dwHighBytesToSeek: int.MaxValue,
                            lpdwLowByteSeeked: out low,
                            lpdwHighByteSeeked: out high,
                            context: ref context))
                        {
                            int error = Marshal.GetLastWin32Error();
                            if (error != WinError.ERROR_SEEK)
                            {
                                throw GetIoExceptionForError(error);
                            }
                        }
                    }

                    return new StreamInfo
                    {
                        Name = name,
                        Type = streamId.dwStreamId,
                        Size = streamId.Size
                    };
                }

                public void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                private void Dispose(bool disposing)
                {
                    this.buffer.Dispose();
                    this.buffer = null;

                    if (context != IntPtr.Zero)
                    {
                        uint bytesRead;
                        if (!BackupRead(
                            hFile: fileHandle,
                            lpBuffer: IntPtr.Zero,
                            nNumberOfBytesToRead: 0,
                            lpNumberOfBytesRead: out bytesRead,
                            bAbort: true,
                            bProcessSecurity: false,
                            context: ref this.context))
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw GetIoExceptionForError(error);
                        }
                    }
                }

                ~BackupReader()
                {
                    this.Dispose(false);
                }
            }

            internal static IEnumerable<AlternateStreamInformation> GetAlternateStreamInformation(string path)
            {
                List<AlternateStreamInformation> streams = new List<AlternateStreamInformation>();
                path = Paths.AddExtendedPrefix(path);
                using (var fileHandle = FileManagement.CreateFile(
                    path,
                    // To look at metadata we don't need read or write access
                    0,
                    FileShare.ReadWrite,
                    FileMode.Open,
                    FileManagement.AllFileAttributeFlags.FILE_FLAG_BACKUP_SEMANTICS))
                {
                    using (BackupReader reader = new BackupReader(fileHandle))
                    {
                        StreamInfo? info;
                        while ((info = reader.GetNextInfo()).HasValue)
                        {
                            if (info.Value.Type == BackupStreamType.BACKUP_ALTERNATE_DATA)
                            {
                                streams.Add(new AlternateStreamInformation { Name = info.Value.Name, Size = info.Value.Size });
                            }
                        }
                    }
                }

                return streams;
            }
        }
    }
}
