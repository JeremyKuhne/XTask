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

    public class ListStreamsTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            foreach (var stream in ExtendedFileService.GetAlternateStreams(GetFullTargetPath()))
            {
                this.Loggers[LoggerType.Result].WriteLine($"Stream '{stream.Name}', Size {stream.Size}");
            }

            return ExitCode.Success;
        }
    }
}