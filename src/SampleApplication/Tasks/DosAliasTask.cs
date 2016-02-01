// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System.Linq;
    using XTask.Tasks;

    public class DosAliasTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            string target = Arguments.Target;
            target = string.IsNullOrWhiteSpace(target) ? null : target;

            var targetPaths =
                from path in ExtendedFileService.QueryDosDeviceNames(target)
                orderby path
                select path;

            int count = 0;
            foreach (string path in targetPaths)
            {
                count++;
                ResultLog.WriteLine(path);
            }

            StatusLog.WriteLine("\nFound {0} paths", count);

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.DosAliasTaskSummary; } }
    }
}
