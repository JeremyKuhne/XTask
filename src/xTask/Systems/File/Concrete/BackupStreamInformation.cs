// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using Windows.Win32.Storage.FileSystem;

namespace XTask.Systems.File.Concrete;

/// <summary>
///  Basic information about a stream
/// </summary>
internal struct BackupStreamInformation
{
    /// <summary>
    ///  Name of the alternate stream
    /// </summary>
    public string Name;

    /// <summary>
    ///  Size of the alternate stream
    /// </summary>
    public long Size;

    /// <summary>
    ///  Stream type.
    /// </summary>
    public WIN_STREAM_ID StreamType;
}
