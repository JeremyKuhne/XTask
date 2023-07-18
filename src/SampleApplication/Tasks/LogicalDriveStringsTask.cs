// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class LogicalDriveStringsTask : FileTask
{
    protected override ExitCode ExecuteFileTask()
    {
        foreach (string drive in ExtendedFileService.GetLogicalDriveStrings())
        {
            ResultLog.WriteLine(drive);
        }

        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.LogicalDriveStringsTaskSummary;
}
