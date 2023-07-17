// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using XTask.Systems.File;
using System.ComponentModel;

namespace xTask.Utility;

internal static unsafe class Error
{
    public static WIN32_ERROR GetLastError() => (WIN32_ERROR)Marshal.GetLastWin32Error();

    // Throws prevent inlining of methods. Try to force methods that throw to not get inlined
    // to ensure callers can be inlined.

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void Throw(this WIN32_ERROR error, string? path = null) => throw error.GetException(path);

    [DoesNotReturn]
    public static void ThrowLastError(string? path = null) => Throw(GetLastError(), path);

    /// <summary>
    ///  Throw the last error code from Windows if <paramref name="result"/> is false.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    internal static void ThrowLastErrorIfFalse(this bool result, string? path = null)
    {
        if (!result)
        {
            GetLastError().Throw(path);
        }
    }

    /// <summary>
    ///  Throw the last error code from Windows if <paramref name="result"/> is false.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    internal static void ThrowLastErrorIfFalse(this BOOL result, string? path = null)
    {
        if (!result)
        {
            GetLastError().Throw(path);
        }
    }

    /// <summary>
    ///  Throw the last error code from Windows if it isn't <paramref name="error"/>.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    public static void ThrowIfLastErrorNot(WIN32_ERROR error, string? path = null)
    {
        WIN32_ERROR lastError = GetLastError();
        if (lastError != error)
        {
            throw lastError.GetException(path);
        }
    }

    /// <summary>
    ///  Convert a Windows error to an HRESULT. [HRESULT_FROM_WIN32]
    /// </summary>
    public static HRESULT ToHRESULT(this WIN32_ERROR error)
    {
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_from_win32
        // return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
        return (HRESULT)(int)((int)error <= 0 ? (int)error : (int)error & 0x0000FFFF | (int)FACILITY_CODE.FACILITY_WIN32 << 16 | 0x80000000);
    }

    /// <summary>
    ///  Turns Windows errors into the appropriate exception (that maps with existing .NET behavior as much as possible).
    /// </summary>
    public static Exception GetException(this WIN32_ERROR error, string? path = null)
    {
        // http://referencesource.microsoft.com/#mscorlib/system/io/__error.cs,142

        string message = path is null
            ? $"{ErrorToString(error)}"
            : $"{ErrorToString(error)} '{path}'";

        return WindowsErrorToException(error, message, path);
    }

    /// <summary>
    ///  Create a descriptive string for the error.
    /// </summary>
    public static string ErrorToString(this WIN32_ERROR error)
    {
        string message = FormatMessage(
            messageId: (uint)error,
            source: HINSTANCE.Null);

        // There are a few defintions for '0', we'll always use ERROR_SUCCESS
        return error == WIN32_ERROR.ERROR_SUCCESS
            ? $"ERROR_SUCCESS ({(uint)error}): {message}"
            : Enum.IsDefined(typeof(WIN32_ERROR), error)
                ? $"{error} ({(uint)error}): {message}"
                : $"Error {error}: {message}";
    }

    internal static Exception WindowsErrorToException(WIN32_ERROR error, string? message, string? path)
    {
        switch (error)
        {
            case WIN32_ERROR.ERROR_FILE_NOT_FOUND:
                return new FileNotFoundException(message, path);
            case WIN32_ERROR.ERROR_PATH_NOT_FOUND:
                return new DirectoryNotFoundException(message);
            case WIN32_ERROR.ERROR_ACCESS_DENIED:
            // Network access doesn't throw UnauthorizedAccess in .NET
            case WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED:
                return new UnauthorizedAccessException(message);
            case WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE:
                return new PathTooLongException(message);
            case WIN32_ERROR.ERROR_INVALID_DRIVE:
                // Not available in Portable libraries
                return new DriveNotFoundException(message);
            case WIN32_ERROR.ERROR_OPERATION_ABORTED:
            case WIN32_ERROR.ERROR_CANCELLED:
                return new OperationCanceledException(message);
            case WIN32_ERROR.ERROR_FILE_EXISTS:
            case WIN32_ERROR.ERROR_ALREADY_EXISTS:
                return new FileExistsException(message);
            case WIN32_ERROR.ERROR_INVALID_PARAMETER:
                return new ArgumentException(message);
            case WIN32_ERROR.ERROR_NOT_SUPPORTED:
            case WIN32_ERROR.ERROR_NOT_SUPPORTED_IN_APPCONTAINER:
                return new NotSupportedException(message);
            case WIN32_ERROR.ERROR_SHARING_VIOLATION:
            case WIN32_ERROR.ERROR_NOT_READY:
            default:
                if (error == (WIN32_ERROR)(int)HRESULT.FVE_E_LOCKED_VOLUME)
                {
                    // return new DriveLockedException(message);
                }

                return new Win32Exception((int)error, message);
        }
    }

    /// <remarks>
    ///  .NET's Win32Exception impements the error code lookup on FormatMessage using FORMAT_MESSAGE_FROM_SYSTEM.
    ///  It won't handle Network Errors (NERR_BASE..MAX_NERR), which come from NETMSG.DLL.
    /// </remarks>
    public static string FormatMessage(
        uint messageId,
        HINSTANCE source = default,
        params string[] args)
    {
        FORMAT_MESSAGE_OPTIONS flags =
            // Let the API allocate the buffer
            FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ALLOCATE_BUFFER
            | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_SYSTEM;

        if (args is null || args.Length == 0)
        {
            flags |= FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_IGNORE_INSERTS;
        }
        else
        {
            flags |= FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ARGUMENT_ARRAY;
        }

        if (!source.IsNull)
        {
            flags |= FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_HMODULE;
        }

        // Don't use line breaks
        flags |= (FORMAT_MESSAGE_OPTIONS)0xFF; // FORMAT_MESSAGE_MAX_WIDTH_MASK

        using StringParameterArray strings = new(args);

        sbyte** sargs = strings;

        PWSTR buffer = default;
        uint result = Interop.FormatMessage(
            dwFlags: flags,
            lpSource: (void*)source.Value,
            dwMessageId: messageId,
            // Do the default language lookup
            dwLanguageId: 0,
            lpBuffer: (PWSTR)(void*)(&buffer),
            nSize: 0,
            Arguments: sargs);

        if (result == 0 || buffer.Value is null)
        {
            if (buffer.Value is not null)
            {
                Interop.LocalFree((HLOCAL)buffer.Value);
            }

            WIN32_ERROR error = GetLastError();
            if (error == WIN32_ERROR.ERROR_MR_MID_NOT_FOUND)
            {
                HRESULT hr = (HRESULT)messageId;
                if (hr.Failed && hr.Facility == FACILITY_CODE.FACILITY_URT)
                {
                    // .NET HRESULT, extract the message
                    string? dotNetMessage = Marshal.GetExceptionForHR((int)hr)?.Message;
                    if (dotNetMessage is not null)
                    {
                        return dotNetMessage;
                    }
                }
            }

            return $"The message for id 0x{messageId:x8} was not found.";
        }

        string message = new(buffer, 0, (int)result);
        if (buffer.Value is not null)
        {
            Interop.LocalFree((HLOCAL)buffer.Value);
        }

        return message;
    }
}