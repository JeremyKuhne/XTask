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

            var fileService = this.GetService<IFileService>();
            string basePath = CurrentDirectory.GetCurrentDirectory();
            string fullPath = fileService.GetFullPath(target, basePath);

            IDirectoryInformation directoryInfo = fileService.GetDirectoryInfo(fullPath);
            CurrentDirectory.SetCurrentDirectory(fullPath);
            this.Loggers[LoggerType.Result].WriteLine(fullPath);

            return ExitCode.Success;
        }
    }
}
