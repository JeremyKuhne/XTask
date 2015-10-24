// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Text;

    internal static partial class NativeMethods
    {
        internal static class Authorization
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa375202.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool AdjustTokenPrivileges(
                    IntPtr TokenHandle,
                    [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
                    ref TOKEN_PRIVILEGES NewState,
                    uint BufferLength,
                    out TOKEN_PRIVILEGES PreviousState,
                    out uint ReturnLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa446671.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetTokenInformation(
                    IntPtr TokenHandle,
                    TOKEN_INFORMATION_CLASS TokenInformationClass,
                    SafeHandle TokenInformation,
                    uint TokenInformationLength,
                    out uint ReturnLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379176.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool LookupPrivilegeNameW(
                    IntPtr lpSystemName,
                    ref LUID lpLuid,
                    StringBuilder lpName,
                    ref uint cchName);

                // https://msdn.microsoft.com/en-us/library/aa379180.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool LookupPrivilegeValueW(
                    string lpSystemName,
                    string lpName,
                    ref LUID lpLuid);

                // https://msdn.microsoft.com/en-us/library/aa379304.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool PrivilegeCheck(
                    SafeCloseHandle ClientToken,
                    ref PRIVILEGE_SET RequiredPrivileges,
                    [MarshalAs(UnmanagedType.Bool)] out bool pfResult);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379590.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool SetThreadToken(
                    IntPtr Thread,
                    SafeCloseHandle Token);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379317.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool RevertToSelf();

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa446617.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool DuplicateTokenEx(
                    SafeCloseHandle hExistingToken,
                    TokenAccessLevels dwDesiredAccess,
                    IntPtr lpTokenAttributes,
                    SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                    TOKEN_TYPE TokenType,
                    ref SafeCloseHandle phNewToken);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379295.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool OpenProcessToken(
                    IntPtr ProcessHandle,
                    TokenAccessLevels DesiredAccesss,
                    out SafeCloseHandle TokenHandle);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379296.aspx
                [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool OpenThreadToken(
                    IntPtr ThreadHandle,
                    TokenAccessLevels DesiredAccess,
                    [MarshalAs(UnmanagedType.Bool)] bool OpenAsSelf,
                    out SafeCloseHandle TokenHandle);
            }

            // In winnt.h
            private const uint PRIVILEGE_SET_ALL_NECESSARY = 1;
            internal const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
            internal const uint SE_PRIVILEGE_ENABLED = 0x00000002;
            internal const uint SE_PRIVILEGE_REMOVED = 0x00000004;
            internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

            private enum TOKEN_INFORMATION_CLASS
            {
                TokenUser = 1,
                TokenGroups,
                TokenPrivileges,
                TokenOwner,
                TokenPrimaryGroup,
                TokenDefaultDacl,
                TokenSource,
                TokenType,
                TokenImpersonationLevel,
                TokenStatistics,
                TokenRestrictedSids,
                TokenSessionId,
                TokenGroupsAndPrivileges,
                TokenSessionReference,
                TokenSandBoxInert,
                TokenAuditPolicy,
                TokenOrigin,
                TokenElevationType,
                TokenLinkedToken,
                TokenElevation,
                TokenHasRestrictions,
                TokenAccessInformation,
                TokenVirtualizationAllowed,
                TokenVirtualizationEnabled,
                TokenIntegrityLevel,
                TokenUIAccess,
                TokenMandatoryPolicy,
                TokenLogonSid,
                TokenIsAppContainer,
                TokenCapabilities,
                TokenAppContainerSid,
                TokenAppContainerNumber,
                TokenUserClaimAttributes,
                TokenDeviceClaimAttributes,
                TokenRestrictedUserClaimAttributes,
                TokenRestrictedDeviceClaimAttributes,
                TokenDeviceGroups,
                TokenRestrictedDeviceGroups,
                TokenSecurityAttributes,
                TokenIsRestricted,
                MaxTokenInfoClass
            }

            // https://msdn.microsoft.com/en-us/library/aa379261.aspx
            [StructLayout(LayoutKind.Sequential)]
            internal struct LUID
            {
                public uint LowPart;
                public uint HighPart;
            }

            // https://msdn.microsoft.com/en-us/library/aa379263.aspx
            [StructLayout(LayoutKind.Sequential)]
            internal struct LUID_AND_ATTRIBUTES
            {
                public LUID Luid;
                public uint Attributes;
            }

            // https://msdn.microsoft.com/en-us/library/aa379307.aspx
            [StructLayout(LayoutKind.Sequential)]
            private struct PRIVILEGE_SET
            {
                public uint PrivilegeCount;
                public uint Control;

                [MarshalAs(UnmanagedType.ByValArray)]
                public LUID_AND_ATTRIBUTES[] Privilege;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379630.aspx
            [StructLayout(LayoutKind.Sequential)]
            private struct TOKEN_PRIVILEGES
            {
                public uint PrivilegeCount;

                [MarshalAs(UnmanagedType.ByValArray)]
                public LUID_AND_ATTRIBUTES[] Privileges;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379572.aspx
            private enum SECURITY_IMPERSONATION_LEVEL
            {
                SecurityAnonymous = 0,
                SecurityIdentification = 1,
                SecurityImpersonation = 2,
                SecurityDelegation = 3
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379633.aspx
            private enum TOKEN_TYPE
            {
                TokenPrimary = 1,
                TokenImpersonation
            }

            internal static IEnumerable<PrivilegeSetting> GetTokenPrivileges(SafeCloseHandle token)
            {
                // Get the buffer size we need
                uint bytesNeeded;
                if (!Private.GetTokenInformation(
                    token.DangerousGetHandle(),
                    TOKEN_INFORMATION_CLASS.TokenPrivileges,
                    EmptySafeHandle.Instance,
                    0,
                    out bytesNeeded))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != WinError.ERROR_INSUFFICIENT_BUFFER)
                        throw GetIoExceptionForError(error);
                }
                else
                {
                    // Didn't need any space for output, let's assume there are no privileges
                    return Enumerable.Empty<PrivilegeSetting>();
                }

                // Initialize the buffer and get the data
                var buffer = new StreamBuffer(bytesNeeded);
                if (!Private.GetTokenInformation(
                    token.DangerousGetHandle(),
                    TOKEN_INFORMATION_CLASS.TokenPrivileges,
                    buffer,
                    (uint)buffer.Length,
                    out bytesNeeded))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }

                // Loop through and get our privileges
                BinaryReader reader = new BinaryReader(buffer, Encoding.Unicode, leaveOpen: true);
                uint count = reader.ReadUInt32();

                var privileges = new List<PrivilegeSetting>();
                StringBuilder sb = new StringBuilder(256);

                for (int i = 0; i < count; i++)
                {
                    LUID luid = new LUID
                    {
                        LowPart = reader.ReadUInt32(),
                        HighPart = reader.ReadUInt32(),
                    };

                    uint length = (uint)sb.Capacity;

                    if (!Private.LookupPrivilegeNameW(IntPtr.Zero, ref luid, sb, ref length))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error);
                    }

                    PrivilegeState state = (PrivilegeState)reader.ReadUInt32();
                    privileges.Add(new PrivilegeSetting(sb.ToString(), state));
                    sb.Clear();
                }

                return privileges;
            }

            /// <summary>
            /// Returns true if the given token has the specified privilege. The privilege may or may not be enabled.
            /// </summary>
            internal static bool HasPrivilege(SafeCloseHandle token, Privileges privilege)
            {
                return GetTokenPrivileges(token).Any(t => t.Privilege == privilege);
            }

            internal static LUID LookupPrivilegeValue(string name)
            {
                LUID luid = new LUID();
                if (!Private.LookupPrivilegeValueW(null, name, ref luid))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, name);
                }

                return luid;
            }

            /// <summary>
            /// Checks if the given privilege is enabled. This does not tell you whether or not it
            /// is possible to get a privilege- most held privileges are not enabled by default.
            /// </summary>
            internal static bool IsPrivilegeEnabled(SafeCloseHandle token, Privileges privilege)
            {
                LUID luid = LookupPrivilegeValue(privilege.ToString());

                var luidAttributes = new LUID_AND_ATTRIBUTES
                {
                    Luid = luid,
                    Attributes = SE_PRIVILEGE_ENABLED
                };

                var set = new PRIVILEGE_SET
                {
                    Control = PRIVILEGE_SET_ALL_NECESSARY,
                    PrivilegeCount = 1,
                    Privilege = new[] { luidAttributes }
                };


                bool result;
                if (!Private.PrivilegeCheck(token, ref set, out result))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, privilege.ToString());
                }

                return result;
            }

            internal static SafeCloseHandle OpenProcessToken(TokenAccessLevels desiredAccess)
            {
                SafeCloseHandle processToken;
                if (!Private.OpenProcessToken(Process.GetCurrentProcess().Handle, desiredAccess, out processToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, desiredAccess.ToString());
                }

                return processToken;
            }

            internal static SafeCloseHandle OpenThreadToken(TokenAccessLevels desiredAccess, bool openAsSelf)
            {
                SafeCloseHandle threadToken;
                if (!Private.OpenThreadToken(NativeMethods.Private.GetCurrentThread(), desiredAccess, openAsSelf, out threadToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != WinError.ERROR_NO_TOKEN)
                        throw GetIoExceptionForError(error, desiredAccess.ToString());

                    SafeCloseHandle processToken = OpenProcessToken(TokenAccessLevels.Duplicate);
                    if (!Private.DuplicateTokenEx(
                        processToken,
                        TokenAccessLevels.Impersonate | TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                        IntPtr.Zero,
                        SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                        TOKEN_TYPE.TokenImpersonation,
                        ref threadToken))
                    {
                        error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error, desiredAccess.ToString());
                    }
                }

                return threadToken;
            }
        }
    }
}
