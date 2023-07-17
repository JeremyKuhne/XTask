// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    /// <summary>
    ///  For objects that can provide clipboard formatted data.
    /// </summary>
    internal interface IClipboardSource
    {
        /// <summary>
        ///  Data formatted for the clipboard- if there is no data available the data member may be null.
        /// </summary>
        ClipboardData GetClipboardData();
    }
}
