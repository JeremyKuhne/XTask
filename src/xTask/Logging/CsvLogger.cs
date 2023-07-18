// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace XTask.Logging;

public sealed class CsvLogger : Logger, IClipboardSource, IDisposable
{
    private readonly MemoryStream _stream;
    private readonly StreamWriter _streamWriter;

    public CsvLogger()
    {
        _stream = new MemoryStream();

        // Do we have to look up the code page here? It doesn't look like Excel supports UTF-8.
        _streamWriter = new StreamWriter(_stream, Encoding.ASCII);
    }

    protected override void WriteInternal(WriteStyle style, string value)
    {
        // CSV logger only logs tables.
        return;
    }

    public override void Write(ITable table)
    {
        // We can't write more than one table, start from a clean slate.
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
        => _stream.Length == 0
            ? default
            : new(new ArraySegment<byte>(_stream.GetBuffer(), 0, (int)_stream.Length), ClipboardFormat.CommaSeparatedValues);

    public void Dispose() => _streamWriter.Dispose();
}
