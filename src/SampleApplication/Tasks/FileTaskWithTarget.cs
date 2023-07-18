// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Logging;
using XTask.Tasks;

namespace XFile.Tasks;

public abstract class FileTaskWithTarget : FileTask
{
    protected override ExitCode CheckPrerequisites()
    {
        if (string.IsNullOrWhiteSpace(Arguments.Target))
        {
            StatusLog.WriteLine(WriteStyle.Error, XFileStrings.ErrorRequiresTarget);
            return ExitCode.InvalidArgument;
        }

        return ExitCode.Success;
    }
}
