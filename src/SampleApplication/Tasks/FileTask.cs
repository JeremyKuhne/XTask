// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using Utility;
    using XTask.Systems.File;
    using XTask.Logging;
    using XTask.Tasks;
    using XTask.Utility;

    public abstract class FileTask : Task
    {
        private bool requiresTarget;
        private static CurrentDirectory currentDirectory;

        protected CurrentDirectory CurrentDirectory
        {
            get
            {
                if (currentDirectory == null)
                {
                    currentDirectory = new CurrentDirectory(ExtendedFileService);
                }
                return currentDirectory;
            }
        }

        protected FileTask(bool requiresTarget = false)
        {
            this.requiresTarget = requiresTarget;
        }

        protected IFileService FileService
        {
            get { return this.GetService<IFileService>(); }
        }

        protected IExtendedFileService ExtendedFileService
        {
            get { return this.GetService<IExtendedFileService>();  }
        }

        protected sealed override ExitCode ExecuteInternal()
        {
            if (requiresTarget && String.IsNullOrWhiteSpace(this.Arguments.Target))
            {
                this.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, XFileStrings.RequiresTargetError);
                return ExitCode.InvalidArgument;
            }

            try
            {
                this.ExecuteFileTask();
            }
            catch (Exception e) when (Exceptions.IsIoException(e))
            {
                this.Loggers[LoggerType.Status].WriteLine(WriteStyle.Error, e.Message);
                return ExitCode.GeneralFailure;
            }

            return ExitCode.Success;
        }

        protected abstract ExitCode ExecuteFileTask();
    }
}
