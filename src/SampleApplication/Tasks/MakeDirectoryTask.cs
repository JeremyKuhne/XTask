// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using System.Runtime.InteropServices;
    using XFile.Utility;
    using XTask.Systems.File;
    using XTask.Interop;
    using XTask.Logging;
    using XTask.Tasks;
    using XTask.Utility;

    public class MakeDirectoryTask : Task
    {
        protected override ExitCode ExecuteInternal()
        {
            string target = this.Arguments.Target;
            if (String.IsNullOrEmpty(target))
            {
                this.Loggers[LoggerType.Status].WriteLine("No target specified.");
                return ExitCode.InvalidArgument;
            }



            //if (NativeMethods.CreateDirectory(target, IntPtr.Zero))
            //{
            //    this.Loggers[LoggerType.Status].WriteLine("Successfully created '{0}'", target);
            //    return ExitCode.Success;
            //}

            this.Loggers[LoggerType.Status].WriteLine(NativeErrorHelper.LastErrorToString(Marshal.GetLastWin32Error()));
            return ExitCode.GeneralFailure;
        }
    }
}
