// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System.Linq;
    using Utility;
    using XTask.Systems.File;
    using XTask.Logging;
    using XTask.Utility;

    public class DirectoryTask : FileTask
    {
        const string DefaultTimeFormat = @"MM/dd/yyyy  hh:mm tt";
        const string DefaultNumberFormat = @"N0";

        protected override ExitCode ExecuteFileTask()
        {
            var fileService = this.GetService<IFileService>();

            Table output = Table.Create(
                new ColumnFormat(1),
                new ColumnFormat(1),
                new ColumnFormat(1, ContentVisibility.Default, Justification.Right),
                new ColumnFormat(4));
            output.HasHeader = false;

            int directoryCount = 0;
            int fileCount = 0;
            ulong totalSize = 0;

            IDirectoryInformation directory = fileService.GetDirectoryInfo(GetFullTargetPath());
            foreach (var subdir in directory.EnumerateDirectories().OrderBy(i => i.Name))
            {
                directoryCount++;
                this.AddToTable(output, subdir);
            }

            foreach (var file in directory.EnumerateFiles().OrderBy(i => i.Name))
            {
                fileCount++;
                totalSize += file.Length;
                this.AddToTable(output, file);
            }

            this.Loggers[LoggerType.Status].WriteLine($" Directory of {directory.Path}");
            this.Loggers[LoggerType.Status].WriteLine();
            this.Loggers[LoggerType.Result].Write(output);
            this.Loggers[LoggerType.Status].WriteLine();
            this.Loggers[LoggerType.Status].WriteLine($" {directoryCount} Dir(s) {fileCount} File(s) {totalSize:N0} bytes");
            return ExitCode.Success;
        }

        private void AddToTable(Table table, IFileSystemInformation info)
        {
            table.AddRow
                (
                    info.LastWriteTime.ToString(DefaultTimeFormat),
                    (info is IDirectoryInformation) ? "<DIR>" : "",
                    (info is IFileInformation) ? ((IFileInformation)info).Length.ToString(DefaultNumberFormat) : "",
                    info.Name
                );
        }
    }
}
