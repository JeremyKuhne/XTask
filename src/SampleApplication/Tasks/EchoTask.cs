// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.FileSystem;
    using XTask.Logging;
    using XTask.Utility;

    public class EchoTask : FileTask
    {
        public EchoTask() : base(requiresTarget: true) { }

        protected override ExitCode ExecuteFileTask()
        {
            string echo = String.Join(" ", Arguments.Targets);

            string target = Arguments.GetOption<string>("target", "t");
            if (target != null)
            {
                target = FileService.GetFullPath(target, CurrentDirectory.GetCurrentDirectory());
                FileService.WriteAllText(target, echo);
            }
            else
            {
                this.Loggers[LoggerType.Result].WriteLine(echo);
            }

            return ExitCode.Success;
        }
    }
}