// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.System.Diagnostics.Debug;
using xTask.Utility;

namespace Windows.Win32.Foundation;

internal partial struct HRESULT
{
    public static explicit operator HRESULT(WIN32_ERROR error)
    {
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_from_win32
        // return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
        return (HRESULT)(int)((int)error <= 0 ? (int)error : (((int)error & 0x0000FFFF) | ((int)FACILITY_CODE.FACILITY_WIN32 << 16) | 0x80000000));
    }

    /// <summary>
    ///  Extracts the code portion of the HRESULT. [HRESULT_CODE]
    /// </summary>
    public int Code
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_code
        // #define HRESULT_CODE(hr)    ((hr) & 0xFFFF)
        => Value & 0xFFFF;

    /// <summary>
    ///  Extracts the facility code of the HRESULT. [HRESULT_FACILITY]
    /// </summary>
    public FACILITY_CODE Facility
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_facility
        // #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
        => (FACILITY_CODE)((Value >> 16) & 0x1fff);

    // COR_* HRESULTs are .NET HRESULTs
    public static readonly HRESULT COR_E_ARGUMENT               = (HRESULT)unchecked((int)0x80070057);
    public static readonly HRESULT TLBX_E_LIBNOTREGISTERED      = (HRESULT)unchecked((int)0x80131165);
    public static readonly HRESULT COR_E_MISSINGFIELD           = (HRESULT)unchecked((int)0x80131511);
    public static readonly HRESULT COR_E_MISSINGMEMBER          = (HRESULT)unchecked((int)0x80131512);
    public static readonly HRESULT COR_E_MISSINGMETHOD          = (HRESULT)unchecked((int)0x80131513);
    public static readonly HRESULT COR_E_NOTSUPPORTED           = (HRESULT)unchecked((int)0x80131515);
    public static readonly HRESULT COR_E_OVERFLOW               = (HRESULT)unchecked((int)0x80131516);
    public static readonly HRESULT COR_E_INVALIDOLEVARIANTTYPE  = (HRESULT)unchecked((int)0x80131531);
    public static readonly HRESULT COR_E_SAFEARRAYTYPEMISMATCH  = (HRESULT)unchecked((int)0x80131533);
    public static readonly HRESULT COR_E_TARGETINVOCATION       = (HRESULT)unchecked((int)0x80131604);
    public static readonly HRESULT COR_E_OBJECTDISPOSED         = (HRESULT)unchecked((int)0x80131622);

    public static implicit operator Exception(HRESULT result)
    {
        return Marshal.GetExceptionForHR(result) ?? new InvalidOperationException("Not a failing result.");
    }

    /// <summary>
    ///  Format an <see cref="HRESULT"/> with a message.
    /// </summary>
    public string ToStringWithDescription()
    {
        bool win32error = Facility == FACILITY_CODE.FACILITY_WIN32;

        string message = Error.FormatMessage(win32error ? (uint)Code : (uint)Value);
        return win32error
            ? $"HRESULT 0x{Value:X8} [{(WIN32_ERROR)Code} ({(uint)Code:D})]: {message}"
            : $"HRESULT 0x{Value:X8} [{Value:D}]: {message}";
    }
}