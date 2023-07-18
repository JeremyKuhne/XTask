// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Systems.File;
using XTask.Tasks;


namespace XFile.Tasks;

[Hidden]
public class TestTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        ResultLog.WriteLine(ExtendedFileService.GetDriveLetter(FileService, GetFullTargetPath()));
        return ExitCode.Success;
    }
}
