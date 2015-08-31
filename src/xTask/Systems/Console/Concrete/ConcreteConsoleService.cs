// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.Console.Concrete
{
    using System;

    /// <summary>
    /// Simple thunk to the System.Console
    /// </summary>
    public class ConcreteConsoleService : IConsoleService
    {
        private static object syncLock;

        static ConcreteConsoleService()
        {
            ConcreteConsoleService.syncLock = new object();
        }

        public void Write(string value)
        {
            // Need to lock to prevent interleaved output from multiple threads (and messed up colors)
            lock (ConcreteConsoleService.syncLock)
            {
                Console.Write(value);
            }
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public string Title
        {
            get { return Console.Title; }
            set { Console.Title = value; }
        }

        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public void ResetColor()
        {
            Console.ResetColor();
        }
    }
}
