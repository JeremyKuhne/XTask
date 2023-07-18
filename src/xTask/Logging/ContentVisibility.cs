// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging;

[Flags]
public enum ContentVisibility
{
    /// <summary>
    ///  No special directives
    /// </summary>
    Default = 0x0000,

    /// <summary>
    ///  All content should be visible if at all possible
    /// </summary>
    ShowAll = 0x0001,

    /// <summary>
    ///  Compress whitespace when displaying in a fixed space (eg, text out)
    /// </summary>
    CompressWhitespace = 0x0010,
}