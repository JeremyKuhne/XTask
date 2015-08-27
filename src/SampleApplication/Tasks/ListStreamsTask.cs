// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.FileSystem;
    using XTask.Logging;
    using XTask.Utility;

    public class ListStreamsTask : FileTask
    {
        public ListStreamsTask() : base(requiresTarget: true) { }

        protected override ExitCode ExecuteFileTask()
        {
            string target = FileService.GetFullPath(Arguments.Target, CurrentDirectory.GetCurrentDirectory());
            foreach (var stream in ExtendedFileService.GetAlternateStreams(target))
            {
                this.Loggers[LoggerType.Result].WriteLine($"Stream '{stream.Name}', Size {stream.Size}");
            }

            return ExitCode.Success;
        }
    }
}