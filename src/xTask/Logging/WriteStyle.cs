// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Logging
{
    [Flags]
    public enum WriteStyle
    {
        Current =       0x00,
        Bold =          0x01,
        Underline =     0x02,
        Italic =        0x04,
        Fixed =         0x10,
        // Proportional =   0x20
        Important =     0x40,
        Critical =      0x80,
        Error =         0x100
    }
}
