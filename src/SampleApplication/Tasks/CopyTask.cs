// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using XTask.Logging;
    using XTask.Systems.File;
    using XTask.Tasks;
    using System.Linq;
    using XTask.Settings;
    public class CopyTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            if (Arguments.Targets.Length < 1 || Arguments.Targets.Length > 2)
            {
                StatusLog.WriteLine(WriteStyle.Error, XFileStrings.ErrorRequiresSourceAndDestination);
            }

            string source = GetFullPath(Arguments.Targets[0]);

            string sourceDirectory = Paths.GetDirectory(source);
            string sourceFile = Paths.GetFileOrDirectoryName(source);

            var directoryInfo = FileService.GetDirectoryInfo(sourceDirectory);
            var files = directoryInfo.EnumerateFiles(sourceFile);

            string destination = GetFullPath(Arguments.Targets.Length == 1 ? "." : Arguments.Targets[1]);
            int fileCount = files.Count();

            if (!FileService.DirectoryExists(destination))
            {
                if (fileCount == 1 && !Paths.EndsInDirectorySeparator(destination))
                {
                    var file = files.First();

                    // Copying a file to another name.
                    StatusLog.WriteLine($"Copying '{file.Name}' to '{destination}'...");
                    FileService.CopyFile(file.Path, destination, Arguments.GetOption<bool>(StandardOptions.Overwrite));
                    StatusLog.WriteLine($"1 file copied.");
                    return ExitCode.Success;
                }

                // We need to create the directory
                FileService.CreateDirectory(destination);
            }

            int filesCopied = 0;
            foreach (var file in files)
            {
                string fullDestination = GetFullPath(Paths.Combine(destination, file.Name));
                StatusLog.WriteLine($"Copying '{file.Name}' to '{destination}'...");
                FileService.CopyFile(file.Path, fullDestination, Arguments.GetOption<bool>(StandardOptions.Overwrite));
                filesCopied++;
            }

            StatusLog.WriteLine($"{filesCopied} file(s) copied.");

            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.CopyTaskSummary; } }

        protected override string OptionDetails { get { return XFileStrings.CopyTaskOptions; } }

        protected override string GeneralHelp { get { return XFileStrings.CopyTaskHelp; } }
    }
}