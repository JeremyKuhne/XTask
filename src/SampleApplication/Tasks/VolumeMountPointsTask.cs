// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Logging;
    using XTask.Utility;

    public class VolumeMountPointsTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            foreach (string pathName in ExtendedFileService.GetVolumeMountPoints(this.Arguments.Target))
            {
                this.Loggers[LoggerType.Result].WriteLine(pathName);
            }

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.VolumeMountPointsTaskSummary; } }
    }
}