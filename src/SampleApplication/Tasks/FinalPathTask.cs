// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class FinalPathTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        ResultLog.WriteLine(ExtendedFileService.GetFinalPath(GetFullTargetPath()));
        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.FinalPathTaskSummary;
}