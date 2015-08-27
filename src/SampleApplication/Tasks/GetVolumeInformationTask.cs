// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using XTask.FileSystem;
    using XTask.Logging;
    using XTask.Utility;

    public class GetVolumeInformationTask : FileTask
    {
        public GetVolumeInformationTask() : base(requiresTarget: true) { }

        protected override ExitCode ExecuteFileTask()
        {
            VolumeInformation info = ExtendedFileService.GetVolumeInformation(this.Arguments.Target);

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

            this.Loggers[LoggerType.Result].Write(table);
            return ExitCode.Success;
        }
    }
}
