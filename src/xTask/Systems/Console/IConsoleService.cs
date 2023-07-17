// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Systems.Console
{
    /// <summary>
    ///  Basic Console access.
    /// </summary>
    public interface IConsoleService
    {
        void Write(string value);
        string ReadLine();
        string Title { get; set; }
        ConsoleColor ForegroundColor { get; set; }
        void ResetColor();
    }
}
