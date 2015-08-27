// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System.IO;
    using System.Text;

    public class CsvLogger : Logger, IClipboardSource
    {
        private StreamWriter streamWriter = new StreamWriter(new MemoryStream(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        protected override void WriteInternal(WriteStyle style, string value)
        {
            // CSV logger only logs tables
            return;
        }

        public override void Write(ITable table)
        {
            // We can't write more than one table, start from a clean slate
            this.streamWriter.BaseStream.Position = 0;
            this.streamWriter.BaseStream.SetLength(0);

            foreach (var row in table.Rows)
            {
                for (int i = 0; i < row.Length - 1; ++i)
                {
                    this.streamWriter.Write("\"{0}\",", row[i]);
                }
                this.streamWriter.WriteLine("\"{0}\"", row[row.Length - 1]);
            }

            this.streamWriter.Flush();
        }

        public ClipboardData GetClipboardData()
        {
            return new ClipboardData { Data = this.streamWriter.BaseStream.Length > 0 ? this.streamWriter.BaseStream : null, Format = ClipboardFormat.CommaSeparatedValues };
        }
    }
}
