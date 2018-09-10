// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using WInterop.Clipboard;

    public static class ClipboardHelper
    {
        /// <summary>
        /// Adds the specified data to the clipboard
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <param name="nameof(data)"/> is null.</exception>
        public static void SetClipboardData(params ClipboardData[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Clipboard.OpenClipboard();
            try
            {
                Clipboard.EmptyClipboard();
                foreach (var item in data)
                {
                    switch (item.Format)
                    {
                        case ClipboardFormat.UnicodeText:
                            Clipboard.SetClipboardUnicodeText(GetCharData(item.Data));
                            break;
                        default:
                            ReadOnlySpan<char> charData = GetCharData(item.Data);
                            if (charData.Length > 0)
                            {
                                Clipboard.SetClipboardAsciiText(charData, GetDataObjectFormatString(item.Format));
                            }
                            else
                            {
                                Clipboard.SetClipboardBinaryData(GetByteData(item.Data), GetDataObjectFormatString(item.Format));
                            }

                            break;
                    }
                }
            }
            finally
            {
                Clipboard.CloseClipboard();
            }
        }

        private static ReadOnlySpan<char> GetCharData(object data)
        {
            if (data is string stringData)
                return stringData.AsSpan();

            if (data is Memory<char> memory)
                return memory.Span;

            if (data is char[] array)
                return array.AsSpan();

            return default;
        }

        private static ReadOnlySpan<byte> GetByteData(object data)
        {
            if (data is Memory<byte> memory)
                return memory.Span;

            if (data is byte[] array)
                return array.AsSpan();

            return default;
        }


        /// <summary>
        /// Gets the proper set/getdata string for the given ClipboardFormat.
        /// </summary>
        private static string GetDataObjectFormatString(ClipboardFormat format)
        {
            switch (format)
            {
                case ClipboardFormat.CommaSeparatedValues:
                    return "Csv";
                case ClipboardFormat.Html:
                    return "HTML Format";
                case ClipboardFormat.RichText:
                    return "Rich Text Format";
                case ClipboardFormat.XmlSpreadsheet:
                    return "Xml Spreadsheet";
                case ClipboardFormat.UnicodeText:
                    return "UnicodeText";
                default:
                    return "Text";
            }
        }
    }
}
