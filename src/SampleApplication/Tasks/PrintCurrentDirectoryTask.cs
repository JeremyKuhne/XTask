// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using Utility;
    using XTask.Logging;
    using XTask.Utility;

    public class PrintCurrentDirectoryTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            this.Loggers[LoggerType.Result].WriteLine(CurrentDirectory.GetCurrentDirectory());
            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.PrintCurrentDirectoryTaskSummary; } }
    }
}
