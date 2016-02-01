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
    using XTask.Tasks;
    using XTask.Utility;

    [XTask.Tasks.Hidden]
    public class TestTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            ResultLog.WriteLine(ExtendedFileService.GetDriveLetter(FileService, GetFullTargetPath()));

            return ExitCode.Success;
        }
    }
}
