// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File
{
    public interface IExtendedFileSystemInformation
    {
        /// <summary>
        ///  Serial number of the volume that contains the file
        /// </summary>
        uint VolumeSerialNumber { get; }

        /// <summary>
        ///  Number of links to the file
        /// </summary>
        uint NumberOfLinks { get; }

        /// <summary>
        ///  File index
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   ReFS uses a 16 byte ID (FILE_ID_128)
        ///  </para>
        /// </remarks>
        ulong FileIndex { get; }
    }
}
