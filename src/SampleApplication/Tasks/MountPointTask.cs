// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Tasks;

namespace XFile.Tasks;

public class MountPointTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        ResultLog.WriteLine(ExtendedFileService.GetMountPoint(GetFullTargetPath()));
        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.MountPointTaskSummary;
}