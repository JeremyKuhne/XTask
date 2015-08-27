// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Logging;
    using XTask.Utility;

    public class ShortPathTask : FileTask
    {
        public ShortPathTask() : base(requiresTarget: true) { }

        protected override ExitCode ExecuteFileTask()
        {
            this.Loggers[LoggerType.Result].WriteLine(ExtendedFileService.GetShortPath(FileService.GetFullPath(this.Arguments.Target)));
            return ExitCode.Success;
        }
    }
}