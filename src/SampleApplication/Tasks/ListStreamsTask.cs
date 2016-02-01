// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Tasks;

    public class ListStreamsTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            foreach (var stream in ExtendedFileService.GetAlternateStreamInformation(GetFullTargetPath()))
            {
                ResultLog.WriteLine($"Stream '{stream.Name}', Size {stream.Size}");
            }

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.ListStreamsTaskSummary; } }
    }
}