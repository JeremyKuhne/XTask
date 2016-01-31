// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text;

    public class XmlSpreadsheetLogger : Logger, IClipboardSource, IDisposable
    {
        private bool _anyData;

        // Excel is super picky about the format of the XML, unable to make XDocument output that made it happy.
        private StreamWriter _streamWriter = new StreamWriter(new MemoryStream(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        protected override void WriteInternal(WriteStyle style, string value)
        {
            // XmlSpreadsheetLogger logger only logs tables
            return;
        }

        private void Initialize()
        {
            _streamWriter.WriteLine(
@"<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<?mso-application progid='Excel.Sheet'?>
<Workbook xmlns='urn:schemas-microsoft-com:office:spreadsheet'
 xmlns:o='urn:schemas-microsoft-com:office:office'
 xmlns:x='urn:schemas-microsoft-com:office:excel'
 xmlns:ss='urn:schemas-microsoft-com:office:spreadsheet'
 xmlns:html='http://www.w3.org/TR/REC-html40'>
 <Worksheet ss:Name='XTaskSheet'>");
        }

        public override void Write(ITable table)
        {
            if (!_anyData) Initialize();
            _streamWriter.WriteLine(@"  <Table>");
            foreach (var row in table.Rows)
            {
                _streamWriter.WriteLine(@"   <Row>");
                foreach (var cell in row)
                {
                    _streamWriter.WriteLine("    <Cell><Data ss:Type='String'>{0}</Data></Cell>", SecurityElement.Escape(cell));
                }
                _streamWriter.WriteLine(@"   </Row>");
            }
            _streamWriter.WriteLine(@"  </Table>");

            _anyData = true;
            _streamWriter.Flush();
        }

        public ClipboardData GetClipboardData()
        {
            if (!_anyData)
            {
                return new ClipboardData { Data = null, Format = ClipboardFormat.XmlSpreadsheet };
            }

            _streamWriter.WriteLine(
@" </Worksheet>
</Workbook>");
            _streamWriter.Flush();

            return new ClipboardData { Data = _streamWriter.BaseStream, Format = ClipboardFormat.XmlSpreadsheet };
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
