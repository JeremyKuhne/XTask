// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Systems.File;
    using XTask.Logging;
    using XTask.Utility;

    public class TypeTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            using (var reader = FileService.CreateReader(GetFullTargetPath()))
            {
                string nextLine = null;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    this.Loggers[LoggerType.Result].WriteLine(nextLine);
                }
            }

            return ExitCode.Success;
        }
    }
}
