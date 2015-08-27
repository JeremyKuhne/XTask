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

    public class GetVolumePathNameTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            string target = this.Arguments.Target;

            if (String.IsNullOrWhiteSpace(target))
            {
                this.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XFileStrings.RequiresTargetError);
                return ExitCode.InvalidArgument;
            }

            this.Loggers[LoggerType.Result].WriteLine(ExtendedFileService.GetVolumePathName(target));

            return ExitCode.Success;
        }
    }
}