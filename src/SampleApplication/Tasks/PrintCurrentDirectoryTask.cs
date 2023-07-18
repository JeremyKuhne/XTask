// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class PrintCurrentDirectoryTask : FileTask
{
    protected override ExitCode ExecuteFileTask()
    {
        ResultLog.WriteLine(FileService.CurrentDirectory);
        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.PrintCurrentDirectoryTaskSummary;
}
