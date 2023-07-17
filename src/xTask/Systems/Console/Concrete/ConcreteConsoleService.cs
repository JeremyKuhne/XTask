// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Systems.Console.Concrete
{
    using Console = System.Console;

    /// <summary>
    ///  Simple thunk to the System.Console
    /// </summary>
    public class ConcreteConsoleService : IConsoleService
    {
        private static readonly object s_SyncLock = new();

        public void Write(string value)
        {
            // Need to lock to prevent interleaved output from multiple threads (and messed up colors)
            lock (s_SyncLock)
            {
                Console.Write(value);
            }
        }

        public string ReadLine() => Console.ReadLine();

        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();
    }
}
