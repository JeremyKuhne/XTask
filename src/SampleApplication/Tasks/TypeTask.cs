// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.FileSystem;
    using XTask.Logging;
    using XTask.Utility;

    public class TypeTask : FileTask
    {
        public TypeTask() : base(requiresTarget: true) { }

        protected override ExitCode ExecuteFileTask()
        {
            using (var reader = FileService.CreateReader(Arguments.Target))
            {
                string nextLine = null;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    this.Loggers[LoggerType.Result].WriteLine(nextLine);
                }
            }

            return ExitCode.Success;
        }
    }
}
