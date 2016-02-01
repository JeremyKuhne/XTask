// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.Logging;
    using XTask.Systems.Console;
    using XTask.Systems.File;
    using XTask.Tasks;
    using XTask.Utility;

    public abstract class FileTask : Task
    {
        protected string GetFullTargetPath()
        {
            return GetFullPath(Arguments.Target);
        }

        protected string GetFullPath(string target)
        {
            return target == null
                ? FileService.GetFullPath(FileService.CurrentDirectory)
                : FileService.GetFullPath(target, FileService.CurrentDirectory);
        }

        protected IFileService FileService
        {
            get { return GetService<IFileService>(); }
        }

        protected IConsoleService ConsoleService
        {
            get { return GetService<IConsoleService>(); }
        }

        protected IExtendedFileService ExtendedFileService
        {
            get { return GetService<IExtendedFileService>();  }
        }

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

        protected virtual ExitCode CheckPrerequisites()
        {
            return ExitCode.Success;
        }

        protected abstract ExitCode ExecuteFileTask();
    }
}
