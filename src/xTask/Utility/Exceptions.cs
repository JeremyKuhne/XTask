// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace XTask.Utility;

public static class Exceptions
{
    /// <summary>
    ///  Returns true if the given exception is an expected exception from IO routines.
    /// </summary>
    public static bool IsIoException(Exception exception)
        => exception is IOException
            // Unfortunately UnauthorizedAccessException and OperationCanceledException come out of
            // IO APIs and don't derive from IOException.
            || exception is UnauthorizedAccessException
            || exception is OperationCanceledException
            // ArgumentException is the saddest of "normal" exceptions to come out of .NET APIs.
            // System.IO.Path.CheckInvalidPathChars() gets called by almost all .NET APIs that
            // deal with paths.
            || exception is ArgumentException;
}
