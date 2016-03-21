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
    using System.Security.Cryptography.X509Certificates;

    internal static partial class NativeMethods
    {
        internal static class Cryptography
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // System Store Locations
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa388136.aspx

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376560.aspx
                [DllImport(Libraries.Crypt32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                internal static extern SafeCertificateStoreHandle CertOpenSystemStoreW(
                    IntPtr hprov,
                    string szSubsystemProtocol);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376026.aspx
                [DllImport(Libraries.Crypt32, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CertCloseStore(
                    IntPtr hCertStore,
                    uint dwFlags);

                // Example C Program: Listing System and Physical Stores
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa382362.aspx

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376060.aspx
                [DllImport(Libraries.Crypt32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CertEnumSystemStoreLocation(
                    uint dwFlags,
                    IntPtr pvArg,
                    CertEnumSystemStoreLocationCallback pfnEnum);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376061.aspx
                [return: MarshalAs(UnmanagedType.Bool)]
                public delegate bool CertEnumSystemStoreLocationCallback(
                    IntPtr pvszStoreLocations,
                    uint dwFlags,
                    IntPtr pvReserved,
                    IntPtr pvArg);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376058.aspx
                [DllImport(Libraries.Crypt32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CertEnumSystemStore(
                    uint dwFlags,
                    IntPtr pvSystemStoreLocationPara,
                    IntPtr pvArg,
                    CertEnumSystemStoreCallback pfnEnum);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376059.aspx
                [return: MarshalAs(UnmanagedType.Bool)]
                public delegate bool CertEnumSystemStoreCallback(
                    IntPtr pvSystemStore,
                    uint dwFlags,
                    IntPtr pStoreInfo,
                    IntPtr pvReserved,
                    IntPtr pvArg);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376055.aspx
                [DllImport(Libraries.Crypt32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CertEnumPhysicalStore(
                    IntPtr pvSystemStore,
                    uint dwFlags,
                    IntPtr pvArg,
                    CertEnumPhysicalStoreCallback pfnEnum);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376056.aspx
                [return: MarshalAs(UnmanagedType.Bool)]
                public delegate bool CertEnumPhysicalStoreCallback(
                    IntPtr pvSystemStore,
                    uint dwFlags,
                    IntPtr pwszStoreName,
                    IntPtr pStoreInfo,
                    IntPtr pvReserved,
                    IntPtr pvArg);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa377568.aspx
                [StructLayout(LayoutKind.Sequential)]
                public struct CERT_SYSTEM_STORE_INFO
                {
                    public uint cbSize;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct CERT_PHYSICAL_STORE_INFO
                {
                    public uint cbSize;
                    public string pszOpenStoreProvider;
                    public uint dwOpenEncodingType;
                    public uint dwOpenFlags;
                    CRYPT_DATA_BLOB OpenParameters;
                    uint dwFlags;
                    uint dwPriority;
                }

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa381414.aspx
                [StructLayout(LayoutKind.Sequential)]
                public struct CRYPT_DATA_BLOB
                {
                    uint cbData;
                    IntPtr pbData;
                }

                // From Wincrypt.h
                internal const uint CERT_STORE_NO_CRYPT_RELEASE_FLAG = 0x00000001;
                internal const uint CERT_STORE_SET_LOCALIZED_NAME_FLAG = 0x00000002;
                internal const uint CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 0x00000004;
                internal const uint CERT_STORE_DELETE_FLAG = 0x00000010;
                internal const uint CERT_STORE_UNSAFE_PHYSICAL_FLAG = 0x00000020;
                internal const uint CERT_STORE_SHARE_STORE_FLAG = 0x00000040;
                internal const uint CERT_STORE_SHARE_CONTEXT_FLAG = 0x00000080;
                internal const uint CERT_STORE_MANIFOLD_FLAG = 0x00000100;
                internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG = 0x00000200;
                internal const uint CERT_STORE_UPDATE_KEYID_FLAG = 0x00000400;
                internal const uint CERT_STORE_BACKUP_RESTORE_FLAG = 0x00000800;
                internal const uint CERT_STORE_READONLY_FLAG = 0x00008000;
                internal const uint CERT_STORE_OPEN_EXISTING_FLAG = 0x00004000;
                internal const uint CERT_STORE_CREATE_NEW_FLAG = 0x00002000;
                internal const uint CERT_STORE_MAXIMUM_ALLOWED_FLAG = 0x00001000;

                internal const uint CRYPT_ASN_ENCODING = 0x00000001;
                internal const uint CRYPT_NDR_ENCODING = 0x00000002;
                internal const uint X509_ASN_ENCODING = 0x00000001;
                internal const uint X509_NDR_ENCODING = 0x00000002;
                internal const uint PKCS_7_ASN_ENCODING = 0x00010000;
                internal const uint PKCS_7_NDR_ENCODING = 0x00020000;

                internal const uint CERT_PHYSICAL_STORE_ADD_ENABLE_FLAG = 0x1;
                internal const uint CERT_PHYSICAL_STORE_OPEN_DISABLE_FLAG = 0x2;
                internal const uint CERT_PHYSICAL_STORE_REMOTE_OPEN_DISABLE_FLAG = 0x4;
                internal const uint CERT_PHYSICAL_STORE_INSERT_COMPUTER_NAME_ENABLE_FLAG = 0x8;

                internal const uint CERT_SYSTEM_STORE_RELOCATE_FLAG = 0x80000000;
                internal const uint CERT_SYSTEM_STORE_LOCATION_MASK = 0x00FF0000;
                internal const int CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;

                internal enum SystemStoreLocationId : uint
                {
                    //  Registry: HKEY_CURRENT_USER or HKEY_LOCAL_MACHINE
                    CERT_SYSTEM_STORE_CURRENT_USER_ID = 1,
                    CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2,
                    //  Registry: HKEY_LOCAL_MACHINE\Software\Microsoft\Cryptography\Services
                    CERT_SYSTEM_STORE_CURRENT_SERVICE_ID = 4,
                    CERT_SYSTEM_STORE_SERVICES_ID = 5,
                    //  Registry: HKEY_USERS
                    CERT_SYSTEM_STORE_USERS_ID = 6,

                    //  Registry: HKEY_CURRENT_USER\Software\Policies\Microsoft\SystemCertificates
                    CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID = 7,
                    //  Registry: HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\SystemCertificates
                    CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID = 8,

                    //  Registry: HKEY_LOCAL_MACHINE\Software\Microsoft\EnterpriseCertificates
                    CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID = 9
                }

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa377575.aspx
                [StructLayout(LayoutKind.Sequential)]
                public struct CERT_SYSTEM_STORE_RELOCATE_PARA
                {
                    /// <summary>
                    /// Can be HKEY hKeyBase
                    /// </summary>
                    public IntPtr pvBase;

                    /// <summary>
                    /// Can be LPCSTR pszSystemStore or LPCWSTR pwszSystemStore
                    /// </summary>
                    public IntPtr pvSystemStore;
                }

                internal static bool SystemStoreLocationCallback(
                    IntPtr pvszStoreLocations,
                    uint dwFlags,
                    IntPtr pvReserved,
                    IntPtr pvArg)
                {
                    GCHandle handle = GCHandle.FromIntPtr(pvArg);
                    var infos = (List<string>)handle.Target;
                    infos.Add(Marshal.PtrToStringUni(pvszStoreLocations));
                    return true;
                }

                internal static bool SystemStoreEnumeratorCallback(
                    IntPtr pvSystemStore,
                    uint dwFlags,
                    IntPtr pStoreInfo,
                    IntPtr pvReserved,
                    IntPtr pvArg)
                {
                    GCHandle handle = GCHandle.FromIntPtr(pvArg);
                    var infos = (List<SystemStoreInfo>)handle.Target;
                    infos.Add(GetSystemNameAndKey(dwFlags, pvSystemStore));
                    return true;
                }

                private static SystemStoreInfo GetSystemNameAndKey(uint dwFlags, IntPtr pvSystemStore)
                {
                    SystemStoreInfo info = new SystemStoreInfo();

                    if ((dwFlags & CERT_SYSTEM_STORE_RELOCATE_FLAG) == CERT_SYSTEM_STORE_RELOCATE_FLAG)
                    {
                        var relocate = Marshal.PtrToStructure<CERT_SYSTEM_STORE_RELOCATE_PARA>(pvSystemStore);
                        var registryHandle = new SafeRegistryHandle(relocate.pvBase, ownsHandle: false);

                        info.Key = RegistryKey.FromHandle(registryHandle).Name;

                        // The name is null terminated
                        info.Name = Marshal.PtrToStringUni(relocate.pvSystemStore);
                    }
                    else
                    {
                        // The name is null terminated
                        info.Name = Marshal.PtrToStringUni(pvSystemStore);
                    }

                    info.Location = (SystemStoreLocation)(dwFlags & CERT_SYSTEM_STORE_LOCATION_MASK);
                    return info;
                }

                internal static bool PhysicalStoreEnumeratorCallback(
                    IntPtr pvSystemStore,
                    uint dwFlags,
                    IntPtr pwszStoreName,
                    IntPtr pStoreInfo,
                    IntPtr pvReserved,
                    IntPtr pvArg)
                {
                    GCHandle handle = GCHandle.FromIntPtr(pvArg);
                    var infos = (List<PhysicalStoreInfo>)handle.Target;

                    PhysicalStoreInfo info = new PhysicalStoreInfo();
                    info.SystemStoreInfo = GetSystemNameAndKey(dwFlags, pvSystemStore);
                    info.PhysicalStoreName = Marshal.PtrToStringUni(pwszStoreName);
                    var physicalInfo = Marshal.PtrToStructure<CERT_PHYSICAL_STORE_INFO>(pStoreInfo);
                    info.ProviderType = physicalInfo.pszOpenStoreProvider;
                    infos.Add(info);

                    return true;
                }
            }

            internal static void CloseStore(SafeCertificateStoreHandle handle)
            {
                if (!Private.CertCloseStore(handle.DangerousGetHandle(), dwFlags: 0))
                {
                    throw GetIoExceptionForError(Marshal.GetLastWin32Error());
                }
            }

            internal static SafeCertificateStoreHandle OpenSystemStore(StoreName storeName)
            {
                string realName = null;
                switch (storeName)
                {
                    case StoreName.AddressBook:
                        realName = "AddressBook";
                        break;
                    case StoreName.AuthRoot:
                        realName = "AuthRoot";
                        break;
                    case StoreName.CertificateAuthority:
                        realName = "CA";
                        break;
                    case StoreName.Disallowed:
                        realName = "Disallowed";
                        break;
                    case StoreName.My:
                        realName = "My";
                        break;
                    case StoreName.Root:
                        realName = "Root";
                        break;
                    case StoreName.TrustedPeople:
                        realName = "TrustedPeople";
                        break;
                    case StoreName.TrustedPublisher:
                        realName = "TrustedPublisher";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(storeName));
                }

                SafeCertificateStoreHandle store = Private.CertOpenSystemStoreW(IntPtr.Zero, realName);
                if (store.IsInvalid)
                {
                    throw GetIoExceptionForError(Marshal.GetLastWin32Error(), realName);
                }
                return store;
            }

            internal enum SystemStoreLocation : uint
            {
                //  Registry: HKEY_CURRENT_USER or HKEY_LOCAL_MACHINE
                CERT_SYSTEM_STORE_CURRENT_USER = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_CURRENT_USER_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,
                CERT_SYSTEM_STORE_LOCAL_MACHINE = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_LOCAL_MACHINE_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,
                //  Registry: HKEY_LOCAL_MACHINE\Software\Microsoft\Cryptography\Services
                CERT_SYSTEM_STORE_CURRENT_SERVICE = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_CURRENT_SERVICE_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,
                CERT_SYSTEM_STORE_SERVICES = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_SERVICES_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,
                //  Registry: HKEY_USERS
                CERT_SYSTEM_STORE_USERS = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_USERS_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,

                //  Registry: HKEY_CURRENT_USER\Software\Policies\Microsoft\SystemCertificates
                CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,
                //  Registry: HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\SystemCertificates
                CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT,

                //  Registry: HKEY_LOCAL_MACHINE\Software\Microsoft\EnterpriseCertificates
                CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE = Private.SystemStoreLocationId.CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID << Private.CERT_SYSTEM_STORE_LOCATION_SHIFT
            }

            [DebuggerDisplay("{Name}")]
            internal struct SystemStoreInfo
            {
                public string Name;
                public string Key;
                public SystemStoreLocation Location;
            }

            [DebuggerDisplay("{SystemStoreInfo.Name} {PhysicalStoreName}")]
            internal struct PhysicalStoreInfo
            {
                public SystemStoreInfo SystemStoreInfo;
                public string PhysicalStoreName;
                public string ProviderType;
                public SystemStoreLocation Location;
            }

            internal static IEnumerable<string> EnumerateSystemStoreLocations()
            {
                var info = new List<string>();
                GCHandle handle = GCHandle.Alloc(info);

                try
                {
                    var callBack = new Private.CertEnumSystemStoreLocationCallback(Private.SystemStoreLocationCallback);
                    Private.CertEnumSystemStoreLocation(
                        dwFlags: 0,
                        pvArg: GCHandle.ToIntPtr(handle),
                        pfnEnum: callBack);
                }
                finally
                {
                    handle.Free();
                }

                return info;
            }

            internal unsafe static IEnumerable<SystemStoreInfo> EnumerateSystemStores(SystemStoreLocation location, string name = null)
            {
                var info = new List<SystemStoreInfo>();
                GCHandle infoHandle = GCHandle.Alloc(info);

                fixed (char* namePointer = string.IsNullOrEmpty(name) ? null : name)
                {
                    try
                    {
                        // To lookup system stores in an alternate location you need to set CERT_SYSTEM_STORE_RELOCATE_FLAG
                        // and pass in the name and alternate location (HKEY) in pvSystemStoreLocationPara.
                        var callBack = new Private.CertEnumSystemStoreCallback(Private.SystemStoreEnumeratorCallback);
                        Private.CertEnumSystemStore(
                            dwFlags: (uint)location,
                            pvSystemStoreLocationPara: (IntPtr)namePointer,
                            pvArg: GCHandle.ToIntPtr(infoHandle),
                            pfnEnum: callBack);
                    }
                    finally
                    {
                        infoHandle.Free();
                    }
                }

                return info;
            }

            internal unsafe static IEnumerable<PhysicalStoreInfo> EnumeratePhysicalStores(SystemStoreLocation location, string systemStoreName)
            {
                var info = new List<PhysicalStoreInfo>();
                GCHandle infoHandle = GCHandle.Alloc(info);

                fixed (char* namePointer = systemStoreName)
                {
                    try
                    {
                        // To lookup system stores in an alternate location you need to set CERT_SYSTEM_STORE_RELOCATE_FLAG
                        // and pass in the name and alternate location (HKEY) in pvSystemStoreLocationPara.
                        var callBack = new Private.CertEnumPhysicalStoreCallback(Private.PhysicalStoreEnumeratorCallback);
                        Private.CertEnumPhysicalStore(
                            pvSystemStore: (IntPtr)namePointer,
                            dwFlags: (uint)location,
                            pvArg: GCHandle.ToIntPtr(infoHandle),
                            pfnEnum: callBack);
                    }
                    finally
                    {
                        infoHandle.Free();
                    }
                }

                return info;
            }
        }
    }
}
