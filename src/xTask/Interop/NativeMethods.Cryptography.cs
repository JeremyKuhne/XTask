// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
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

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa376058.aspx
                // [DllImport(Libraries.Crypt32, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
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
        }
    }
}
