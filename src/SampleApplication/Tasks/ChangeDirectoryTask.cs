// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class ChangeDirectoryTask : FileTask
{
    protected override ExitCode ExecuteFileTask()
    {
        string target = Arguments.Target;
        target ??= ".";

        string fullPath = ExtendedFileService.GetFinalPath(GetFullPath(target));

        FileService.CurrentDirectory = fullPath;
        ResultLog.WriteLine(FileService.CurrentDirectory);

        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.ChangeDirectorySummary;
}
