// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using XTask.Logging;
using XTask.Systems.File;
using XTask.Tasks;

namespace XFile.Tasks
{
    public class VolumeInformationTask : FileTaskWithTarget
    {
        protected override ExitCode ExecuteFileTask()
        {
            VolumeInformation info = ExtendedFileService.GetVolumeInformation(Arguments.Target);

            Table table = Table.Create(new ColumnFormat(1, ContentVisibility.ShowAll, Justification.Right), new ColumnFormat(1));
            table.HasHeader = false;
            table.AddRow("Volume Name", info.VolumeName);
            table.AddRow("Serial Number", info.VolumeSerialNumber.ToString());
            table.AddRow("Max Component Length", info.MaximumComponentLength.ToString());
            table.AddRow("File System Name", info.FileSystemName.ToString());
            foreach (var value in Enum.GetValues(typeof(FileSystemFeature)))
            {
                FileSystemFeature feature = (FileSystemFeature)value;
                if ((feature & info.FileSystemFlags) == feature)
                {
                    table.AddRow(feature.ToString(), "true");
                }
                else
                {
                    table.AddRow(feature.ToString(), "false");
                }
            }

            ResultLog.Write(table);
            return ExitCode.Success;
        }

        public override string Summary { get { return XFileStrings.VolumeInformationTaskSummary; } }
    }
}
