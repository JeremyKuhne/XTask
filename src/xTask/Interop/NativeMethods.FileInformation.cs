// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using XTask.Systems.File;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    internal static partial class NativeMethods
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364232.aspx
        internal static class FileInformation
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // This isn't an actual Windows type, we have to separate it out as the size of IntPtr varies by architecture
                // and we can't specify the size at compile time to offset the Information pointer in the status block.
                [StructLayout(LayoutKind.Explicit)]
                public struct IO_STATUS
                {
                    [FieldOffset(0)]
                    int Status;

                    [FieldOffset(0)]
                    IntPtr Pointer;
                }

                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff550671.aspx
                [StructLayout(LayoutKind.Sequential)]
                public struct IO_STATUS_BLOCK
                {
                    IO_STATUS Status;
                    IntPtr Information;
                }

                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff728840.aspx
                public enum FILE_INFORMATION_CLASS
                {
                    FileDirectoryInformation = 1,
                    FileFullDirectoryInformation,
                    FileBothDirectoryInformation,
                    FileBasicInformation,
                    FileStandardInformation,
                    FileInternalInformation,
                    FileEaInformation,
                    FileAccessInformation,
                    FileNameInformation,
                    FileRenameInformation,
                    FileLinkInformation,
                    FileNamesInformation,
                    FileDispositionInformation,
                    FilePositionInformation,
                    FileFullEaInformation,
                    FileModeInformation,
                    FileAlignmentInformation,
                    FileAllInformation,
                    FileAllocationInformation,
                    FileEndOfFileInformation,
                    FileAlternateNameInformation,
                    FileStreamInformation,
                    FilePipeInformation,
                    FilePipeLocalInformation,
                    FilePipeRemoteInformation,
                    FileMailslotQueryInformation,
                    FileMailslotSetInformation,
                    FileCompressionInformation,
                    FileObjectIdInformation,
                    FileCompletionInformation,
                    FileMoveClusterInformation,
                    FileQuotaInformation,
                    FileReparsePointInformation,
                    FileNetworkOpenInformation,
                    FileAttributeTagInformation,
                    FileTrackingInformation,
                    FileIdBothDirectoryInformation,
                    FileIdFullDirectoryInformation,
                    FileValidDataLengthInformation,
                    FileShortNameInformation,
                    FileIoCompletionNotificationInformation,
                    FileIoStatusBlockRangeInformation,
                    FileIoPriorityHintInformation,
                    FileSfioReserveInformation,
                    FileSfioVolumeInformation,
                    FileHardLinkInformation,
                    FileProcessIdsUsingFileInformation,
                    FileNormalizedNameInformation,
                    FileNetworkPhysicalNameInformation,
                    FileIdGlobalTxDirectoryInformation,
                    FileIsRemoteDeviceInformation,
                    FileUnusedInformation,
                    FileNumaNodeInformation,
                    FileStandardLinkInformation,
                    FileRemoteProtocolInformation,
                    FileRenameInformationBypassAccessCheck,
                    FileLinkInformationBypassAccessCheck,
                    FileVolumeNameInformation,
                    FileIdInformation,
                    FileIdExtdDirectoryInformation,
                    FileReplaceCompletionInformation,
                    FileHardLinkFullIdInformation,
                    FileIdExtdBothDirectoryInformation,
                    FileMaximumInformation
                }

                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff567052.aspx
                // http://www.pinvoke.net/default.aspx/ntdll/NtQueryInformationFile.html
                [DllImport(Libraries.Ntdll, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern int NtQueryInformationFile(
                    SafeFileHandle FileHandle,
                    out IO_STATUS_BLOCK IoStatusBlock,
                    IntPtr FileInformation,
                    uint Length,
                    FILE_INFORMATION_CLASS FileInformationClass);
            }

            internal static string GetFileName(SafeFileHandle fileHandle)
            {
                // https://msdn.microsoft.com/en-us/library/windows/hardware/ff545817.aspx

                //  typedef struct _FILE_NAME_INFORMATION
                //  {
                //      ULONG FileNameLength;
                //      WCHAR FileName[1];
                //  } FILE_NAME_INFORMATION, *PFILE_NAME_INFORMATION;

                using (NativeBuffer nb = new NativeBuffer())
                {
                    int status = NtStatus.STATUS_BUFFER_OVERFLOW;
                    uint nameLength = Paths.MaxPath * sizeof(char);

                    Private.IO_STATUS_BLOCK ioStatus;

                    while (status == NtStatus.STATUS_BUFFER_OVERFLOW)
                    {
                        // Add space for the FileNameLength
                        nb.EnsureByteCapacity(nameLength + sizeof(uint));

                        status = Private.NtQueryInformationFile(
                            FileHandle: fileHandle,
                            IoStatusBlock: out ioStatus,
                            FileInformation: nb.DangerousGetHandle(),
                            Length: checked((uint)nb.ByteCapacity),
                            FileInformationClass: Private.FILE_INFORMATION_CLASS.FileNameInformation);

                        nameLength = (uint)NativeBufferReader.ReadInt(nb, 0);
                    }

                    if (!Errors.NT_SUCCESS(status))
                    {
                        throw GetIoExceptionForError(Errors.NtStatusToWinError(status));
                    }

                    // The string isn't null terminated so we have to explicitly pass the size
                    string value = NativeBufferReader.ReadString(nb, sizeof(uint), checked((int)nameLength) / sizeof(char));
                    return value;
                }
            }
        }
    }
}
