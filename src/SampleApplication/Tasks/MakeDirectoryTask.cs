﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Tasks;

namespace XFile.Tasks
{
    public class MakeDirectoryTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            FileService.CreateDirectory(GetFullTargetPath());
            return ExitCode.Success;
        }

        public override string Summary => XFileStrings.MakeDirectoryTaskSummary;
    }
}
