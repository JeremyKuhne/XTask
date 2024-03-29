﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class RemoveDirectoryTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        string path = GetFullTargetPath();
        if (!GetService<IConsoleService>().QueryYesNo(XFileStrings.QueryDeleteDirectory, path))
        {
            return ExitCode.Canceled;
        }

        FileService.DeleteDirectory(path, deleteChildren: true);
        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.RemoveDirectoryTaskSummary;
}
