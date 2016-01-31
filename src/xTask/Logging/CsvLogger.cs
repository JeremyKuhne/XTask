// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using System.IO;
    using System.Text;

    public class CsvLogger : Logger, IClipboardSource, IDisposable
    {
        private StreamWriter _streamWriter = new StreamWriter(new MemoryStream(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        protected override void WriteInternal(WriteStyle style, string value)
        {
            // CSV logger only logs tables
            return;
        }

        public override void Write(ITable table)
        {
            // We can't write more than one table, start from a clean slate
            _streamWriter.BaseStream.Position = 0;
            _streamWriter.BaseStream.SetLength(0);

            foreach (var row in table.Rows)
            {
                for (int i = 0; i < row.Length - 1; ++i)
                {
                    _streamWriter.Write("\"{0}\",", row[i]);
                }
                _streamWriter.WriteLine("\"{0}\"", row[row.Length - 1]);
            }

            _streamWriter.Flush();
        }

        public ClipboardData GetClipboardData()
        {
            return new ClipboardData { Data = _streamWriter.BaseStream.Length > 0 ? _streamWriter.BaseStream : null, Format = ClipboardFormat.CommaSeparatedValues };
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _streamWriter.Dispose();
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
