// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.Utility;

    public class MakeDirectoryTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            FileService.CreateDirectory(GetFullTargetPath());
            return ExitCode.Success;
        }
    }
}
