// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using XTask.Logging;
using XTask.Systems.Console;
using XTask.Systems.File;
using XTask.Tasks;
using XTask.Utility;

namespace XFile.Tasks;

public abstract class FileTask : Task
{
    protected string GetFullTargetPath() => GetFullPath(Arguments.Target);

    protected string GetFullPath(string target)
        => target is null
            ? FileService.GetFullPath(FileService.CurrentDirectory)
            : FileService.GetFullPath(target, FileService.CurrentDirectory);

    protected IFileService FileService => GetService<IFileService>();

    protected IConsoleService ConsoleService => GetService<IConsoleService>();

    protected IExtendedFileService ExtendedFileService => GetService<IExtendedFileService>();

    protected sealed override ExitCode ExecuteInternal()
    {
        ExitCode check = CheckPrerequisites();
        if (check != ExitCode.Success) return check;

        try
        {
            ExecuteFileTask();
        }
        catch (Exception e) when (Exceptions.IsIoException(e))
        {
            StatusLog.WriteLine(WriteStyle.Error, e.Message);
            return ExitCode.GeneralFailure;
        }

        return ExitCode.Success;
    }

    protected virtual ExitCode CheckPrerequisites() => ExitCode.Success;

    protected abstract ExitCode ExecuteFileTask();
}
