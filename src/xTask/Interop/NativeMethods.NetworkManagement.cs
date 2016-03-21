// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static partial class NativeMethods
    {
        internal static class NetworkManagement
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // NET_API_STATUS is a DWORD (uint) but we'll use int to fall in with what .NET does

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370304.aspx
                [DllImport(Libraries.Netapi32, ExactSpelling = true)]
                internal static extern int NetApiBufferFree(
                    IntPtr Buffer);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370440.aspx
                [DllImport(Libraries.Netapi32, CharSet = CharSet.Unicode, ExactSpelling = true)]
                internal static extern int NetLocalGroupEnum(
                    string servername,
                    uint level,
                    out SafeNetApiBufferHandle bufptr,
                    uint prefmaxlen,
                    out uint entriesread,
                    out uint totalentries,
                    IntPtr resumehandle);

                internal const uint MAX_PREFERRED_LENGTH = unchecked((uint)-1);

                // LOCALGROUP_INFO_0 is simply a pointer to a Unicode name string
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370275.aspx
                //
                // LOCALGROUP_INFO_1 is two Unicode pointers, one to the name and one to the comment
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370277.aspx

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370601.aspx
                [DllImport(Libraries.Netapi32, CharSet = CharSet.Unicode, ExactSpelling = true)]
                internal static extern int NetLocalGroupGetMembers(
                    string servername,
                    string localgroupname,
                    uint level,
                    out SafeNetApiBufferHandle bufptr,
                    uint prefmaxlen,
                    out uint entriesread,
                    out uint totalentries,
                    IntPtr resumehandle);

                // LOCALGROUP_MEMBERS_INFO_0 is a pointer to a SID
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370278.aspx

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370279.aspx
                [StructLayout(LayoutKind.Sequential)]
                public struct LOCALGROUP_MEMBERS_INFO_1 
                {
                    public IntPtr lgrmi1_sid;
                    public SID_NAME_USE lgrmi1_sidusage;
                    public IntPtr lgrmi1_name;
                }

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa370280.aspx
                [StructLayout(LayoutKind.Sequential)]
                public struct LOCALGROUP_MEMBERS_INFO_2
                {
                    public IntPtr lgrmi2_sid;
                    public SID_NAME_USE lgrmi2_sidusage;
                    public IntPtr lgrmi2_domainandname;
                }

                // LOCALGROUP_MEMBERS_INFO_3 is a pointer to a Unicode DOMAIN\name string

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379601.aspx
                internal enum SID_NAME_USE
                {
                    SidTypeUser = 1,
                    SidTypeGroup,
                    SidTypeDomain,
                    SidTypeAlias,
                    SidTypeWellKnownGroup,
                    SidTypeDeletedAccount,
                    SidTypeInvalid,
                    SidTypeUnknown,
                    SidTypeComputer,
                    SidTypeLabel
                }
            }

            internal enum SidType
            {
                User = Private.SID_NAME_USE.SidTypeUser,
                Group = Private.SID_NAME_USE.SidTypeGroup,
                Domain = Private.SID_NAME_USE.SidTypeDomain,
                Alias = Private.SID_NAME_USE.SidTypeAlias,
                WellKnownGroup = Private.SID_NAME_USE.SidTypeWellKnownGroup,
                DeletedAccount = Private.SID_NAME_USE.SidTypeDeletedAccount,
                Invalid = Private.SID_NAME_USE.SidTypeInvalid,
                Unknown = Private.SID_NAME_USE.SidTypeUnknown,
                Computer = Private.SID_NAME_USE.SidTypeComputer,
                Label = Private.SID_NAME_USE.SidTypeLabel
            }

            internal static void NetApiBufferFree(SafeNetApiBufferHandle buffer)
            {
                int result = Private.NetApiBufferFree(buffer.DangerousGetHandle());
                if (result != WinError.NERR_Success)
                {
                    throw GetIoExceptionForError(result);
                }
            }

            internal static IEnumerable<string> EnumerateLocalGroups(string server = null)
            {
                var groups = new List<string>();

                SafeNetApiBufferHandle buffer;
                uint entriesRead;
                uint totalEntries;

                int result = Private.NetLocalGroupEnum(
                    servername: server,
                    level: 0,
                    bufptr: out buffer,
                    prefmaxlen: Private.MAX_PREFERRED_LENGTH,
                    entriesread: out entriesRead,
                    totalentries: out totalEntries,
                    resumehandle: IntPtr.Zero);

                if (result != WinError.NERR_Success)
                {
                    throw GetIoExceptionForError(result, server);
                }

                foreach (IntPtr pointer in ReadStructsFromBuffer<IntPtr>(buffer, entriesRead))
                {
                    groups.Add(Marshal.PtrToStringUni(pointer));
                }

                return groups;
            }

            internal static IEnumerable<MemberInfo> EnumerateGroupUsers(string groupName, string server = null)
            {
                var members = new List<MemberInfo>();

                SafeNetApiBufferHandle buffer;
                uint entriesRead;
                uint totalEntries;

                int result = Private.NetLocalGroupGetMembers(
                    servername: server,
                    localgroupname: groupName,
                    level: 1,
                    bufptr: out buffer,
                    prefmaxlen: Private.MAX_PREFERRED_LENGTH,
                    entriesread: out entriesRead,
                    totalentries: out totalEntries,
                    resumehandle: IntPtr.Zero);

                if (result != WinError.NERR_Success)
                {
                    throw GetIoExceptionForError(result, server);
                }

                foreach (Private.LOCALGROUP_MEMBERS_INFO_1 info in ReadStructsFromBuffer<Private.LOCALGROUP_MEMBERS_INFO_1>(buffer, entriesRead))
                {
                    members.Add(new MemberInfo
                    {
                        Name = Marshal.PtrToStringUni(info.lgrmi1_name),
                        AccountType = (SidType)info.lgrmi1_sidusage
                    });
                }

                return members;
            }

            private static IEnumerable<T> ReadStructsFromBuffer<T>(SafeNetApiBufferHandle buffer, uint count) where T : struct
            {
                uint size = (uint)Marshal.SizeOf<T>();
                var items = new List<T>((int)count);
                buffer.Initialize(numElements: count, sizeOfEachElement: size);

                for (uint i = 0; i < count; i++)
                {
                    var current = buffer.Read<T>(i * size);
                    items.Add(current);
                }

                return items;
            }

            [DebuggerDisplay("{Name} {AccountType}")]
            internal struct MemberInfo
            {
                public string Name;
                public SidType AccountType;
            }
        }
    }
}
