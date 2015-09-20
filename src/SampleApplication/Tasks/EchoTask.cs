// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using System.Linq;
    using XTask.Systems.File;
    using XTask.Logging;
    using XTask.Utility;

    public class EchoTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            string target = Arguments.GetOption<string>("target", "t");
            int redirector = Array.IndexOf<string>(Arguments.Targets, ">");

            if (target == null && redirector == -1)
            {
                ResultLog.WriteLine(String.Join(" ", Arguments.Targets));
                return ExitCode.Success;
            }

            if (target == null)
            {
                if (redirector == Arguments.Targets.Length - 1)
                {
                    StatusLog.WriteLine(WriteStyle.Error, XFileStrings.ErrorNoTargetSpecified);
                    return ExitCode.InvalidArgument;
                }

                target = Arguments.Targets[redirector + 1];
            }

            target = GetFullPath(target);
            FileService.WriteAllText(target, String.Join(" ", redirector == -1 ? Arguments.Targets : Arguments.Targets.Take(redirector)));

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.EchoTaskSummary; } }
    }
}