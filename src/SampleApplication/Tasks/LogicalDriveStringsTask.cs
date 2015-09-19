// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Logging;
    using XTask.Utility;

    public class LogicalDriveStringsTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            foreach (string drive in ExtendedFileService.GetLogicalDriveStrings())
            {
                this.Loggers[LoggerType.Result].WriteLine(drive);
            }

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.LogicalDriveStringsTaskSummary; } }
    }
}
