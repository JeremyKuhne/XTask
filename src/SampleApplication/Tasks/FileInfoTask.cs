﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks;

public class FileInfoTask : FileTaskWithTarget
{
    protected override ExitCode ExecuteFileTask()
    {
        string path = GetFullTargetPath();
        var fileInfo = FileService.GetFileInfo(path);
        var extendedInfo = fileInfo as IExtendedFileSystemInformation;

        Table table = Table.Create(new ColumnFormat(1, ContentVisibility.ShowAll, Justification.Right), new ColumnFormat(1));
        table.HasHeader = false;
        table.AddRow("Name", fileInfo.Name);
        table.AddRow("Path", fileInfo.Path);
        table.AddRow("Length", fileInfo.Length.ToString());
        table.AddRow("Attributes", fileInfo.Attributes.ToString());
        table.AddRow("Creation Time", fileInfo.CreationTime.ToString());
        table.AddRow("Last Write Time", fileInfo.LastWriteTime.ToString());
        table.AddRow("Last Access Time", fileInfo.LastAccessTime.ToString());
        table.AddRow("Volume Serial Number", extendedInfo.VolumeSerialNumber.ToString());
        table.AddRow("File Index", extendedInfo.FileIndex.ToString());
        table.AddRow("Number of Links", extendedInfo.NumberOfLinks.ToString());
        ResultLog.Write(table);
        return ExitCode.Success;
    }

    public override string Summary => XFileStrings.FileInfoTaskSummary;
}
