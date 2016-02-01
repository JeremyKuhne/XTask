// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using Tasks;

    /// <summary>
    /// Used to centrally handle app failure state
    /// </summary>
    [Serializable]
    public abstract class TaskException : Exception
    {
        public TaskException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Returns the most appropriate exit code for the exception
        /// </summary>
        public abstract ExitCode ExitCode { get; }
    }
}