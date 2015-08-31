// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

namespace XTask.Systems.File
{
    /// <summary>
    /// Basic information about an alternate stream
    /// </summary>
    public struct AlternateStreamInformation
    {
        /// <summary>
        /// Name of the alternate stream
        /// </summary>
        public string Name;

        /// <summary>
        /// Size of the alternate stream
        /// </summary>
        public ulong Size;
    }
}
