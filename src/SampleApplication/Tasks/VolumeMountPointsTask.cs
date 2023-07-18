// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class VolumeMountPointsTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        foreach (string pathName in ExtendedFileService.GetVolumeMountPoints(Arguments.Target))
        {
            ResultLog.WriteLine(pathName);
        }

        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.VolumeMountPointsTaskSummary;
}