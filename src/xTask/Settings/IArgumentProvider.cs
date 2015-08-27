// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;
    using System.Collections.Generic;

    public interface IArgumentProvider
    {
        /// <summary>
        /// The first non-option parameter, if any
        /// </summary>
        string Target { get; }

        /// <summary>
        /// The specified command
        /// </summary>
        string Command { get; }

        /// <summary>
        /// Ordered list of all non-option parameters
        /// </summary>
        string[] Targets { get; }

        /// <summary>
        /// Returns the specified option or default for the type if not set
        /// (use nullable if you want to know if a primitive type is set)
        /// </summary>
        /// <param name="optionNames">Name for the option and aliases, in priority order</param>
        T GetOption<T>(params string[] optionNames);

        /// <summary>
        /// True if the user asked for help
        /// </summary>
        bool HelpRequested { get; }

        /// <summary>
        /// Get all set options
        /// </summary>
        IReadOnlyDictionary<string, string> Options { get; }
    }
}