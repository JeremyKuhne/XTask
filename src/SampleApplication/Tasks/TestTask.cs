// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XTask.Interop;
    using XTask.Logging;
    using XTask.Systems.File;
    using XTask.Systems.File.Concrete.Flex;
    using XTask.Utility;

    [XTask.Tasks.Hidden]
    public class TestTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            Loggers[LoggerType.Result].WriteLine(ExtendedFileService.GetDriveLetter(this.FileService, GetFullTargetPath()));

            return ExitCode.Success;
        }
    }
}
