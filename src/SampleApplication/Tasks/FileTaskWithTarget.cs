// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.Logging;
    using XTask.Utility;

    public abstract class FileTaskWithTarget : FileTask
    {
        protected override ExitCode CheckPrerequisites()
        {
            if (String.IsNullOrWhiteSpace(this.Arguments.Target))
            {
                StatusLog.WriteLine(WriteStyle.Error, XFileStrings.ErrorRequiresTarget);
                return ExitCode.InvalidArgument;
            }

            return ExitCode.Success;
        }
    }
}
