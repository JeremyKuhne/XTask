// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using XTask.Utility;
    using System.IO;
    using System.Security;
    using System.Text;

    public class XmlSpreadsheetLogger : Logger, IClipboardSource
    {
        private bool anyData;

        // Excel is super picky about the format of the XML, unable to make XDocument output that made it happy.
        private StreamWriter streamWriter = new StreamWriter(new MemoryStream(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        protected override void WriteInternal(WriteStyle style, string value)
        {
            // XmlSpreadsheetLogger logger only logs tables
            return;
        }

        private void Initialize()
        {
            this.streamWriter.WriteLine(
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
            if (!this.anyData) this.Initialize();
            this.streamWriter.WriteLine(@"  <Table>");
            foreach (var row in table.Rows)
            {
                this.streamWriter.WriteLine(@"   <Row>");
                foreach (var cell in row)
                {
                    this.streamWriter.WriteLine("    <Cell><Data ss:Type='String'>{0}</Data></Cell>", SecurityElement.Escape(cell));
                }
                this.streamWriter.WriteLine(@"   </Row>");
            }
            this.streamWriter.WriteLine(@"  </Table>");

            this.anyData = true;
            this.streamWriter.Flush();
        }

        public ClipboardData GetClipboardData()
        {
            if (!this.anyData)
            {
                return new ClipboardData { Data = null, Format = ClipboardFormat.XmlSpreadsheet };
            }

            this.streamWriter.WriteLine(
@" </Worksheet>
</Workbook>");
            this.streamWriter.Flush();

            return new ClipboardData { Data = this.streamWriter.BaseStream, Format = ClipboardFormat.XmlSpreadsheet };
        }
    }
}
