// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class TypeTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        using var reader = FileService.CreateReader(GetFullTargetPath());
        string nextLine = null;
        while ((nextLine = reader.ReadLine()) is not null)
        {
            ResultLog.WriteLine(nextLine);
        }

        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.MakeDirectoryTaskSummary;
}
