// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Security;

namespace XTask.Logging;

public class XmlSpreadsheetLogger : Logger, IClipboardSource, IDisposable
{
    // Excel is super picky about the format of the XML, unable to make XDocument output that made it happy.

    private bool _anyData;

    private readonly MemoryStream _stream;
    private readonly StreamWriter _streamWriter;

    public XmlSpreadsheetLogger()
    {
        _stream = new MemoryStream();
        _streamWriter = new StreamWriter(_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    protected override void WriteInternal(WriteStyle style, string value)
    {
        // XmlSpreadsheetLogger logger only logs tables
        return;
    }

    private void Initialize()
    {
        _streamWriter.WriteLine("""
                <?xml version='1.0' encoding='utf-8' standalone='yes'?>
                <?mso-application progid='Excel.Sheet'?>
                <Workbook xmlns='urn:schemas-microsoft-com:office:spreadsheet'
                 xmlns:o='urn:schemas-microsoft-com:office:office'
                 xmlns:x='urn:schemas-microsoft-com:office:excel'
                 xmlns:ss='urn:schemas-microsoft-com:office:spreadsheet'
                 xmlns:html='http://www.w3.org/TR/REC-html40'>
                 <Worksheet ss:Name='XTaskSheet'>
                """);
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
            return default;
        }

        _streamWriter.WriteLine("""
                </Worksheet>
               </Workbook>
               """);
        _streamWriter.Flush();

        return new ClipboardData(
            new ArraySegment<byte>(_stream.GetBuffer(), 0, (int)_stream.Length),
            ClipboardFormat.XmlSpreadsheet);
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
