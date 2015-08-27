// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.IO;

    public static class Exceptions
    {
        /// <summary>
        /// Returns true if the given exception is an expected exception from IO routines.
        /// </summary>
        public static bool IsIoException(Exception exception)
        {
            return exception is IOException
                // Unfortunately AccessViolationException and OperationCanceledException come out of
                // IO APIs and don't derive from IOException.
                || exception is AccessViolationException
                || exception is OperationCanceledException
                // ArgumentException is the saddest of "normal" exceptions to come out of .NET APIs.
                // System.IO.Path.CheckInvalidPathChars() gets called by almost all .NET APIs that
                // deal with paths.
                || exception is ArgumentException;
        }
    }
}
