// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public static class NativeErrorHelper
    {
        // All defines referenced in this file are from winerror.h unless otherwise specified

        // Windows Error Codes specification [MS-ERREF]
        // https://msdn.microsoft.com/en-us/library/cc231196.aspx

        // Structure of COM Error Codes
        // https://msdn.microsoft.com/en-us/library/ms690088

        // How do I convert an HRESULT to a Win32 error code?
        // http://blogs.msdn.com/b/oldnewthing/archive/2006/11/03/942851.aspx

        // How to: Map HRESULTs and Exceptions
        // https://msdn.microsoft.com/en-us/library/9ztbc5s1.aspx

        private const int FACILITY_WIN32 = 7;

        /// <summary>
        /// Extracts the code portion of the specified HRESULT
        /// </summary>
        private static int HRESULT_CODE(int hr)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms679761.aspx
            // #define HRESULT_CODE(hr)    ((hr) & 0xFFFF)
            return hr & 0xFFFF;
        }

        /// <summary>
        /// Extracts the facility of the specified HRESULT
        /// </summary>
        private static int HRESULT_FACILITY(int hr)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms680579.aspx
            // #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
            return (hr >> 16) & 0x1fff;
        }

        private static int HRESULT_SEVERITY(int hr)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms693761.aspx
            // #define HRESULT_SEVERITY(hr)  (((hr) >> 31) & 0x1)  
            return (((hr) >> 31) & 0x1);
        }

        private static int HRESULT_FROM_WIN32(int error)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms680746(v=vs.85).aspx
            // return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
            return error <= 0 ? error : unchecked((int)((error & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000));
        }

        internal static int GetHResultForWindowsError(int error)
        {
            return HRESULT_FROM_WIN32(error);
        }

        private static int ConvertHResult(int result)
        {
            if (HRESULT_FACILITY(result) == FACILITY_WIN32)
            {
                // Win32 Error, extract the code
                return HRESULT_CODE(result);
            }
            return result;
        }

        /// <summary>
        /// Try to get the string for an HRESULT
        /// </summary>
        public static string HResultToString(int result)
        {
            string message;
            if (HRESULT_FACILITY(result) == FACILITY_WIN32)
            {
                // Win32 Error, extract the code
                message = NativeErrorHelper.FormatMessage(HRESULT_CODE(result));
            }
            else
            {
                // Hope that we get a rational IErrorInfo
                Exception exception = Marshal.GetExceptionForHR(result);
                message = exception.Message;
            }

            return String.Format(
                CultureInfo.CurrentUICulture,
                "HRESULT {0:D} [0x{0:X}]: {1}",
                result,
                message);
        }

        /// <summary>
        /// Try to get the error message for GetLastError result
        /// </summary>
        public static string LastErrorToString(int error)
        {
            return String.Format(
                CultureInfo.CurrentUICulture,
                "Error {0}: {1}",
                error,
                NativeErrorHelper.FormatMessage(error));
        }

        private static string FormatMessage(int error)
        {
            // FormatMessage function:
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms679351.aspx

            // .NET's Win32Exception impements the error code lookup on FormatMessage using FORMAT_MESSAGE_FROM_SYSTEM.
            // It won't handle Network Errors (NERR_BASE..MAX_NERR), which come from NETMSG.DLL.
            return new Win32Exception(error).Message;
        }
    }
}
