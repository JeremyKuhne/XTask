// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.Systems.File;
    using XTask.Logging;
    using XTask.Utility;

    public class EchoTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            string echo = String.Join(" ", Arguments.Targets);

            string target = Arguments.GetOption<string>("target", "t");
            if (target != null)
            {
                target = GetFullPath(target);
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