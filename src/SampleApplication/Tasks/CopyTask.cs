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

    public class CopyTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            if (Arguments.Targets.Length != 2)
            {
                StatusLog.WriteLine(WriteStyle.Error, XFileStrings.ErrorRequiresSourceAndDestination);
            }

            string source = GetFullPath(Arguments.Targets[0]);
            string destination = GetFullPath(Arguments.Targets[1]);

            StatusLog.WriteLine($"Copying '{source}' to '{destination}'...");
            FileService.CopyFile(source, destination);
            StatusLog.WriteLine("1 file copied.");

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.CopyTaskSummary; } }
    }
}