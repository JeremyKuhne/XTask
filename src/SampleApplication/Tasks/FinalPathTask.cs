// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Logging;
    using XTask.Utility;

    public class FinalPathTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            this.Loggers[LoggerType.Result].WriteLine(ExtendedFileService.GetFinalPath(this.GetFullTargetPath()));
            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.FinalPathTaskSummary; } }
    }
}