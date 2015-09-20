// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using Utility;
    using XTask.Systems.File;
    using XTask.Logging;
    using XTask.Utility;

    public class ChangeDirectoryTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            string target = this.Arguments.Target;
            if (target == null)
            {
                target = ".";
            }
            target = Paths.AddTrailingSeparator(target);

            string fullPath = GetFullPath(target);

            IDirectoryInformation directoryInfo = this.FileService.GetDirectoryInfo(fullPath);
            FileService.CurrentDirectory = fullPath;
            this.Loggers[LoggerType.Result].WriteLine(fullPath);

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.ChangeDirectorySummary; } }
    }
}
