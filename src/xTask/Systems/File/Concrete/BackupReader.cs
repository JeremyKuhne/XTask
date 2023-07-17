// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using Microsoft.Win32.SafeHandles;
using System;
using Windows.Support;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using xTask.Utility;

namespace XTask.Systems.File.Concrete;

internal unsafe class BackupReader : IDisposable
{
    private void* _context = null;
    private readonly SafeFileHandle _fileHandle;

    // BackupReader requires us to read the header and its string separately. Given packing, the
    // string starts a uint in from the end.
    private static readonly unsafe uint s_headerSize = (uint)sizeof(WIN32_STREAM_ID) - sizeof(uint);

    public BackupReader(SafeFileHandle fileHandle) => _fileHandle = fileHandle;

    public BackupStreamInformation Current { get; private set; }

    public bool GetNextInfo()
    {
        HANDLE handle = (HANDLE)_fileHandle.DangerousGetHandle();

        WIN32_STREAM_ID streamId = default;
        uint bytesRead;

        // WIN32_STREAM_ID has a trailing anysize char array. We need the header minus the trailing array to
        // get the size of the trailing array to read it properly. As the sizeof(WIN32_STREAM_ID) includes
        // packing space we need to subtract the size of a uint, not just a sizeof(char).

        fixed (void** c = &_context)
        {
            Interop.BackupRead(
                hFile: handle,
                lpBuffer: (byte*)&streamId,
                nNumberOfBytesToRead: (uint)sizeof(WIN32_STREAM_ID) - sizeof(uint),
                lpNumberOfBytesRead: &bytesRead,
                bAbort: false,
                bProcessSecurity: true,
                lpContext: c).ThrowLastErrorIfFalse();
        }

        // Exit if at the end
        if (bytesRead == 0)
        {
            Current = default;
            return false;
        }

        string? name = null;

        uint size = streamId.dwStreamNameSize;
        if (size > 0)
        {
            int sizeInChars = (int)size / sizeof(char);
            using BufferScope<char> buffer = new(
                stackalloc char[(int)Interop.MAX_PATH],
                sizeInChars);

            fixed (void** c = &_context)
            fixed (char* b = buffer)
            {
                Interop.BackupRead(
                    hFile: handle,
                    lpBuffer: (byte*)b,
                    nNumberOfBytesToRead: streamId.dwStreamNameSize,
                    lpNumberOfBytesRead: &bytesRead,
                    bAbort: false,
                    bProcessSecurity: true,
                    lpContext: c).ThrowLastErrorIfFalse();

                name = buffer.Slice(0, sizeInChars).ToString();
            }
        }

        if (streamId.Size > 0)
        {
            uint lowSeeked;
            uint highSeeked;

            // Move to the next header, if any. Seeking will fail with ERROR_SEEK if we read off the stream
            // and will position at the next header (WIN32_STREAM_ID). As such we just try to seek the max value.
            fixed (void** c = &_context)
            {
                if (!Interop.BackupSeek(
                    hFile: handle,
                    dwLowBytesToSeek: uint.MaxValue,
                    dwHighBytesToSeek: uint.MaxValue,
                    lpdwLowByteSeeked: &lowSeeked,
                    lpdwHighByteSeeked: &highSeeked,
                    lpContext: c))
                {
                    Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SEEK);
                }
            }
        }

        GC.KeepAlive(_fileHandle);

        Current = new BackupStreamInformation
        {
            Name = name ?? string.Empty,
            StreamType = streamId.dwStreamId,
            Size = streamId.Size
        };

        return true;
    }

    public void Dispose()
    {
        if (_context is null)
        {
            return;
        }

        fixed (void** c = &_context)
        {
            uint bytesRead;

            // Free the context memory
            Interop.BackupRead(
                hFile: (HANDLE)_fileHandle.DangerousGetHandle(),
                lpBuffer: null,
                nNumberOfBytesToRead: 0,
                lpNumberOfBytesRead: &bytesRead,
                bAbort: true,
                bProcessSecurity: false,
                lpContext: c).ThrowLastErrorIfFalse();

            GC.KeepAlive(_fileHandle);
        }

        _context = null;
    }
}
