// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    public enum LoggerType
    {
        /// <summary>
        ///  Process information, not results.
        /// </summary>
        Status,

        /// <summary>
        ///  Diagnostic information.
        /// </summary>
        Diagnostic,

        /// <summary>
        ///  The data requested (not the process of getting it).
        /// </summary>
        /// <remarks>
        ///  <summary>
        ///   The idea here is that the output of commands goes to a result log. For example, getting a list of files-
        ///   the files would be output to "Result". This allows meaningful information to be put on the clipboard.
        ///   Any other informational log statements should go to "Status" or "Diagnostic".
        ///  </summary>
        /// </remarks>
        Result
    }
}