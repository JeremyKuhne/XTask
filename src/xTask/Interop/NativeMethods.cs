// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Utility;
    using XTask.Systems.File;

    internal static partial class NativeMethods
    {
        // Design Guidelines and Notes
        // ===========================
        //
        // Keep P/Invokes private and provide internal wrappers that do appropriate preparation and error handling.
        //
        // In/Out attributes implicitly applied for parameter & return values:
        //
        //      None Specified -> [In]
        //      out            -> [Out]
        //      ref            -> [In],[Out]
        //      return value   -> [Out]
        //
        // [PreserveSig(false)]
        //
        //  When this is explicitly set to false (the default is true), failed HRESULT return values will be turned into Exceptions
        //  (and the return value in the definition becomes null as a result)
        //
        // [DllImport(SetLastError=true)]
        //
        //  Set this if the API uses GetLastError and use Marshal.GetLastWin32Error to get the value. If the API sets a condition
        //  that says it has an error, get the error before making other calls to avoid inadvertently having it overwritten.
        //
        // [DllImport(ExactSpelling=true)]
        //
        //  Set this and the framework will avoid looking for an "A"/"W" version. (See NDirectMethodDesc::FindEntryPoint)
        //
        // Strings:
        // --------
        //
        // Strings are marshalled as LPTSTR by default, which means it will match the CharSet property in the DllImport attribute.
        // The CharSet is, by default, ANSI, which isn't appropriate for anything post Windows 9x (which isn't supported by .NET
        // anymore). As such, the mapping is actually as follows:
        //
        //      CharSet.None    -> Ansi
        //      CharSet.Ansi    -> Ansi
        //      CharSet.Unicode -> Unicode
        //      CharSet.Auto    -> Unicode
        //
        // When the CharSet is Unicode or the argument is explicitly marked as [MarshalAs(UnmanagedType.LPWSTR)], and the string is
        // is passed by value (not ref/out) the string can be pinned and used directly by managed code (rather than copied).
        //
        // The CLR will use CoTaskMemFree by default to free strings that are passed as [Out] or SysStringFree for strings that are marked
        // as BSTR.
        //
        // (StringBuilder - ILWSTRBufferMarshaler)
        // By default it is passed as [In, Out]. ALWAYS specify the capacity in advance and ensure it is large enough for API in question.
        //
        // StringBuilder is guaranteed to have a null that is not counted in the capacity. As such the count of characters when using as a
        // character buffer is Capacity + 1.

        // Useful Interop Links
        // ====================
        //
        // "Windows Data Types"                  http://msdn.microsoft.com/en-us/library/aa383751.aspx
        // "Windows Data Types for Strings"      http://msdn.microsoft.com/en-us/library/dd374131.aspx
        // "Data Type Ranges"                    http://msdn.microsoft.com/en-us/library/s3f49ktz.aspx
        // "MarshalAs Attribute"                 http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.marshalasattribute.aspx
        // "GetLastError and managed code"       http://blogs.msdn.com/b/adam_nathan/archive/2003/04/25/56643.aspx
        // "Copying and Pinning"                 https://msdn.microsoft.com/en-us/library/23acw07k.aspx
        // "Default Marshalling for Strings"     https://msdn.microsoft.com/en-us/library/s9ts558h.aspx
        // "Marshalling between Managed and Unmanaged Code" (MSDN Magazine January 2008)
        //
        // PInvoke code is in dllimport, method, and ilmarshalers in coreclr\src\vm.

        // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
        [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
        private static class Private
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms683188.aspx
            [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
            internal static extern uint GetEnvironmentVariableW(
                string lpName,
                StringBuilder lpBuffer,
                uint nSize);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms686206.aspx
            [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetEnvironmentVariableW(
                string lpName,
                string lpValue);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms683182.aspx
            [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            internal static extern IntPtr GetCurrentThread();

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211.aspx
            [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(
                IntPtr handle);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684179.aspx
            [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
            internal static extern SafeLibraryHandle LoadLibraryExW(
                string lpFileName,
                IntPtr hReservedNull,
                LoadLibraryFlags dwFlags);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms683152.aspx
            [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool FreeLibrary(
                IntPtr hModule);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647486.aspx
            [DllImport(Libraries.User32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
            unsafe internal static extern int LoadStringW(
                SafeLibraryHandle hInstance,
                int uID,
                out char* lpBuffer,
                int nBufferMax);

            // This API is only available in ANSI
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms683212.aspx
            [DllImport(Libraries.Kernel32, CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, BestFitMapping = false)]
            internal static extern IntPtr GetProcAddress(
                SafeLibraryHandle hModule,
                [MarshalAs(UnmanagedType.LPStr)] string methodName);
        }

        internal static class Libraries
        {
            internal const string Kernel32 = "kernel32.dll";
            internal const string Advapi32 = "advapi32.dll";
            internal const string User32 = "user32.dll";
        }

        /// <summary>
        /// Uses the stringbuilder cache and increases the buffer size if needed. Handles path prepending as needed.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
        private static string BufferPathInvoke(string path, Func<string, StringBuilder, uint> invoker, bool utilizeExtendedSyntax = true)
        {
            if (path == null) return null;

            bool hadExtendedPrefix = path.StartsWith(Paths.ExtendedPathPrefix, StringComparison.Ordinal);
            bool addedExtendedPrefix = false;
            if (utilizeExtendedSyntax && !hadExtendedPrefix && (path.Length > Paths.MaxPath))
            {
                path = Paths.ExtendedPathPrefix + path;
                addedExtendedPrefix = true;
            }

            StringBuilder buffer = StringBuilderCache.Instance.Acquire();
            uint returnValue = invoker(path, buffer);

            while (returnValue > (uint)buffer.Capacity)
            {
                // Need more room for the output string
                buffer.EnsureCapacity((int)returnValue);
                returnValue = invoker(path, buffer);
            }

            if (returnValue == 0)
            {
                // Failed
                int error = Marshal.GetLastWin32Error();
                throw GetIoExceptionForError(error, path);
            }

            bool nowHasExtendedPrefix = buffer.StartsWithOrdinal(Paths.ExtendedPathPrefix);
            if (addedExtendedPrefix || (!hadExtendedPrefix && nowHasExtendedPrefix))
            {
                // Remove the prefix
                buffer.Remove(0, Paths.ExtendedPathPrefix.Length);
            }

            return StringBuilderCache.Instance.ToStringAndRelease(buffer);
        }

        /// <summary>
        /// Uses the stringbuilder cache and increases the buffer size if needed.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
        internal static string BufferInvoke(Func<StringBuilder, uint> invoker, string value = null, Func<int, bool> shouldThrow = null)
        {
            StringBuilder buffer = StringBuilderCache.Instance.Acquire();
            uint returnValue = invoker(buffer);

            while (returnValue > (uint)buffer.Capacity)
            {
                // Need more room for the output string
                buffer.EnsureCapacity((int)returnValue);
                returnValue = invoker(buffer);
            }

            if (returnValue == 0)
            {
                // Failed
                int error = Marshal.GetLastWin32Error();

                if (shouldThrow != null && !shouldThrow(error))
                {
                    return null;
                }
                throw GetIoExceptionForError(error, value);
            }

            return StringBuilderCache.Instance.ToStringAndRelease(buffer);
        }

        internal static void SetEnvironmentVariable(string name, string value)
        {
            if (!Private.SetEnvironmentVariableW(name, value))
            {
                int error = Marshal.GetLastWin32Error();
                throw GetIoExceptionForError(error, name);
            }
        }

        internal static string GetEnvironmentVariable(string name)
        {
            return BufferInvoke(
                sb => Private.GetEnvironmentVariableW(name, sb, (uint)sb.Capacity + 1),
                name,
                error => error != WinError.ERROR_ENVVAR_NOT_FOUND);
        }

        internal static void CloseHandle(IntPtr handle)
        {
            if (!Private.CloseHandle(handle))
            {
                int error = Marshal.GetLastWin32Error();
                throw GetIoExceptionForError(error);
            }
        }

        internal static ulong HighLowToLong(uint high, uint low)
        {
            return ((ulong)high) << 32 | ((ulong)low & 0xFFFFFFFFL);
        }

        internal static DateTime GetDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            return DateTime.FromFileTime((((long)fileTime.dwHighDateTime) << 32) + fileTime.dwLowDateTime);
        }

        //[DllImport("ntdll.dll", SetLastError = true)]
        //public static extern uint NtQueryObject(IntPtr handle, ObjectInformationClass objectInformationClass,
        //    IntPtr objectInformation, uint objectInformationLength, out uint returnLength);

        internal static bool FreeLibrary(SafeLibraryHandle handle)
        {
            return Private.FreeLibrary(handle.DangerousGetHandle());
        }

        internal static SafeLibraryHandle LoadLibrary(string path, LoadLibraryFlags flags)
        {
            SafeLibraryHandle handle = Private.LoadLibraryExW(path, IntPtr.Zero, flags);
            if (handle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                throw GetIoExceptionForError(error, path);
            }
            return handle;
        }

        unsafe internal static string LoadString(SafeLibraryHandle library, int identifier)
        {
            // Passing 0 will give us a read only handle to the string resource
            char* buffer;
            int result = Private.LoadStringW(library, identifier, out buffer, 0);
            if (result <= 0)
            {
                int error = Marshal.GetLastWin32Error();
                throw GetIoExceptionForError(error, identifier.ToString());
            }

            // Null is not included in the result
            return new string(buffer, 0, result);
        }

        internal static DelegateType GetFunctionDelegate<DelegateType>(SafeLibraryHandle library, string methodName)
        {
            IntPtr method = Private.GetProcAddress(library, methodName);
            if (method == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw GetIoExceptionForError(error, methodName);
            }

            return (DelegateType)(object)Marshal.GetDelegateForFunctionPointer(method, typeof(DelegateType));
        }
    }
}
