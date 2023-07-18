// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using XTask.Logging;
using XTask.Systems.File;
using XTask.Tasks;

namespace XFile.Tasks;

public class DirectoryTask : FileTask
{
    private const string DefaultTimeFormat = @"MM/dd/yyyy  hh:mm tt";
    private const string DefaultNumberFormat = @"N0";

    protected override ExitCode ExecuteFileTask()
    {
        var fileService = GetService<IFileService>();

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
            AddToTable(output, subdir);
        }

        foreach (var file in directory.EnumerateFiles().OrderBy(i => i.Name))
        {
            fileCount++;
            totalSize += file.Length;
            AddToTable(output, file);
        }

        StatusLog.WriteLine($" Directory of {directory.Path}");
        StatusLog.WriteLine();
        ResultLog.Write(output);
        StatusLog.WriteLine();
        StatusLog.WriteLine($" {directoryCount} Dir(s) {fileCount} File(s) {totalSize:N0} bytes");
        return ExitCode.Success;
    }

    private static void AddToTable(Table table, IFileSystemInformation info)
    {
        table.AddRow
            (
                info.LastWriteTime.ToString(DefaultTimeFormat),
                (info is IDirectoryInformation) ? "<DIR>" : "",
                (info is IFileInformation information) ? information.Length.ToString(DefaultNumberFormat) : "",
                info.Name
            );
    }

    public override string Summary => XFileStrings.DirectoryTaskSummary;
}
