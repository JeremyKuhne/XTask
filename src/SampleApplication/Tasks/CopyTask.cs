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

    public class CopyTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            if (Arguments.Targets.Length != 2)
            {
                this.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XFileStrings.RequiresSourceAndDestinationError);
            }

            string source = FileService.GetFullPath(Arguments.Targets[0], CurrentDirectory.GetCurrentDirectory());
            string destination = FileService.GetFullPath(Arguments.Targets[1], CurrentDirectory.GetCurrentDirectory());

            this.Loggers[LoggerType.Status].WriteLine($"Copying '{source}' to '{destination}'...");
            FileService.CopyFile(source, destination);
            this.Loggers[LoggerType.Status].WriteLine("1 file copied.");

            return ExitCode.Success;
        }
    }
}