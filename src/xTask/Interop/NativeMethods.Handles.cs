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
    using XTask.Systems.File;

    internal static partial class NativeMethods
    {
        internal static class Handles
        {
            // Windows Kernel Architecture Internals
            // http://research.microsoft.com/en-us/um/redmond/events/wincore2010/Dave_Probert_1.pdf


            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211.aspx
                [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool CloseHandle(
                    IntPtr handle);

                // http://forum.sysinternals.com/howto-enumerate-handles_topic18892.html

                // https://msdn.microsoft.com/en-us/library/bb432383.aspx
                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff567062.aspx
                [DllImport(Libraries.Ntdll, SetLastError = true, ExactSpelling = true)]
                internal static extern int NtQueryObject(
                    IntPtr Handle,
                    OBJECT_INFORMATION_CLASS ObjectInformationClass,
                    IntPtr ObjectInformation,
                    uint ObjectInformationLength,
                    out uint ReturnLength);

                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff550964.aspx
                internal enum OBJECT_INFORMATION_CLASS
                {
                    ObjectBasicInformation,

                    // Undocumented directly, returns a UNICODE_STRING
                    // https://msdn.microsoft.com/en-us/library/windows/hardware/ff548474(v=vs.85).aspx
                    ObjectNameInformation,

                    ObjectTypeInformation,

                    // Undocumented
                    // https://ntquery.wordpress.com/2014/03/30/anti-debug-ntqueryobject/#more-21
                    ObjectTypesInformation
                }

                // https://msdn.microsoft.com/en-us/library/bb432383(v=vs.85).aspx
                //
                //  IoQueryFileDosDeviceName wraps this
                //  https://msdn.microsoft.com/en-us/library/windows/hardware/ff548474.aspx
                //      typedef struct _OBJECT_NAME_INFORMATION
                //      {
                //          UNICODE_STRING Name;
                //      } OBJECT_NAME_INFORMATION, *POBJECT_NAME_INFORMATION;
                //
                // There isn't any point in wrapping this as it is simply a UNICODE_STRING directly
                // followed by it's backing buffer.

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa446633.aspx
                // ACCESS_MASK
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa374892.aspx
                [StructLayout(LayoutKind.Sequential)]
                internal struct GENERIC_MAPPING
                {
                    uint GenericRead;
                    uint GenericWrite;
                    uint GenericExecute;
                    uint GenericAll;
                }

                // The full struct isn't officially documented, names may be wrong.
                //
                //  https://msdn.microsoft.com/en-us/library/windows/hardware/ff551947.aspx
                //      typedef struct __PUBLIC_OBJECT_TYPE_INFORMATION
                //      {
                //          UNICODE_STRING TypeName;
                //          ULONG Reserved[22];    // reserved for internal use
                //      } PUBLIC_OBJECT_TYPE_INFORMATION, *PPUBLIC_OBJECT_TYPE_INFORMATION;
                //
                [StructLayout(LayoutKind.Sequential)]
                internal struct OBJECT_TYPE_INFORMATION
                {
                    public UNICODE_STRING TypeName;

                    // All below are not officially documented, names may be incorrect

                    public uint TotalNumberOfObjects;
                    public uint TotalNumberOfHandles;
                    public uint TotalPagedPoolUsage;
                    public uint TotalNonPagedPoolUsage;
                    public uint TotalNamePoolUsage;
                    public uint TotalHandleTableUsage;
                    public uint HighWaterNumberOfObjects;
                    public uint HighWaterNumberOfHandles;
                    public uint HighWaterPagedPoolUsage;
                    public uint HighWaterNonPagedPoolUsage;
                    public uint HighWaterNamePoolUsage;
                    public uint HighWaterHandleTableUsage;
                    public uint InvalidAttributes;
                    public GENERIC_MAPPING GenericMapping;
                    public uint ValidAccessMask;
                    public byte SecurityRequired;
                    public byte MaintainHandleCount;
                    public byte TypeIndex;
                    public byte ReservedByte;
                    public uint PoolType;
                    public uint DefaultPagedPoolCharge;
                    public uint DefaultNonPagedPoolCharge;
                }

                internal static uint ObjectTypeInformationSize = (uint)Marshal.SizeOf<OBJECT_TYPE_INFORMATION>();

                // The full struct isn't officially documented, names may be wrong.
                //
                //  http://msdn.microsoft.com/en-us/library/windows/hardware/ff551944.aspx
                //      typedef struct _PUBLIC_OBJECT_BASIC_INFORMATION
                //      {
                //          ULONG Attributes;
                //          ACCESS_MASK GrantedAccess;
                //          ULONG HandleCount;
                //          ULONG PointerCount;
                //          ULONG Reserved[10];    // reserved for internal use
                //      } PUBLIC_OBJECT_BASIC_INFORMATION, *PPUBLIC_OBJECT_BASIC_INFORMATION;
                //
                // ACCESS_MASK
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa374892.aspx
                //
                [StructLayout(LayoutKind.Sequential)]
                internal struct OBJECT_BASIC_INFORMATION
                {
                    public uint Attributes;
                    public uint GrantedAccess;
                    public uint HandleCount;
                    public uint PointerCount;
                    public uint PagedPoolCharge;
                    public uint NonPagedPoolCharge;
                    public uint Reserved1;
                    public uint Reserved2;
                    public uint Reserved3;
                    public uint NameInfoSize;
                    public uint TypeInfoSize;
                    public uint SecurityDescriptorSize;
                    public long CreationTime;
                }

                //  typedef struct _OBJECT_TYPES_INFORMATION
                //  {
                //      ULONG NumberOfTypes;
                //      OBJECT_TYPE_INFORMATION TypeInformation;
                //  } OBJECT_TYPES_INFORMATION, *POBJECT_TYPES_INFORMATION;
            }

            internal static void CloseHandle(IntPtr handle)
            {
                if (!Private.CloseHandle(handle))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }
            }

            unsafe internal static string GetObjectName(IntPtr windowsObject)
            {
                // IoQueryFileDosDeviceName wraps this for file handles, but requires calling ExFreePool to free the allocated memory
                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff548474.aspx
                //
                // http://undocumented.ntinternals.net/index.html?page=UserMode%2FUndocumented%20Functions%2FNT%20Objects%2FType%20independed%2FOBJECT_NAME_INFORMATION.html
                //
                //  typedef struct _OBJECT_NAME_INFORMATION
                //  {
                //       UNICODE_STRING Name;
                //       WCHAR NameBuffer[0];
                //  } OBJECT_NAME_INFORMATION, *POBJECT_NAME_INFORMATION;
                //
                // The above definition means the API expects a buffer where it can stick a UNICODE_STRING with the buffer immediately following.

                using (NativeBuffer nb = new NativeBuffer())
                {
                    int status = NtStatus.STATUS_BUFFER_OVERFLOW;
                    uint returnLength = Paths.MaxPath * sizeof(char);

                    while (status == NtStatus.STATUS_BUFFER_OVERFLOW || status == NtStatus.STATUS_BUFFER_TOO_SMALL)
                    {
                        nb.EnsureByteCapacity(returnLength);

                        status = Private.NtQueryObject(
                            Handle: windowsObject,
                            ObjectInformationClass: Private.OBJECT_INFORMATION_CLASS.ObjectNameInformation,
                            ObjectInformation: nb.DangerousGetHandle(),
                            ObjectInformationLength: checked((uint)nb.ByteCapacity),
                            ReturnLength: out returnLength);
                    }

                    if (!Errors.NT_SUCCESS(status))
                    {
                        throw GetIoExceptionForError(Errors.NtStatusToWinError(status));
                    }

                    var info = Marshal.PtrToStructure<UNICODE_STRING>(nb.DangerousGetHandle());

                    // The string isn't null terminated so we have to explicitly pass the size
                    return new string(info.Buffer, 0, info.Length / sizeof(char));
                }
            }

            unsafe internal static string GetObjectTypeName(IntPtr windowsObject)
            {
                using (NativeBuffer nb = new NativeBuffer())
                {
                    int status = NtStatus.STATUS_BUFFER_OVERFLOW;

                    // We'll initially give room for 50 characters for the type name
                    uint returnLength = Private.ObjectTypeInformationSize + 50 * sizeof(char);

                    while (status == NtStatus.STATUS_BUFFER_OVERFLOW || status == NtStatus.STATUS_BUFFER_TOO_SMALL || status == NtStatus.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        nb.EnsureByteCapacity(returnLength);

                        status = Private.NtQueryObject(
                            Handle: windowsObject,
                            ObjectInformationClass: Private.OBJECT_INFORMATION_CLASS.ObjectTypeInformation,
                            ObjectInformation: nb.DangerousGetHandle(),
                            ObjectInformationLength: checked((uint)nb.ByteCapacity),
                            ReturnLength: out returnLength);
                    }

                    if (!Errors.NT_SUCCESS(status))
                    {
                        throw GetIoExceptionForError(Errors.NtStatusToWinError(status));
                    }

                    var info = Marshal.PtrToStructure<Private.OBJECT_TYPE_INFORMATION>(nb.DangerousGetHandle());

                    // The string isn't null terminated so we have to explicitly pass the size
                    return new string(info.TypeName.Buffer, 0, info.TypeName.Length / sizeof(char));
                }
            }

        }
    }
}
