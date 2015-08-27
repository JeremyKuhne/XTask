// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using Forms = System.Windows.Forms;

    public static class Clipboard
    {
        /// <summary>
        /// Adds the specified data to the clipboard
        /// </summary>
        public static void AddToClipboard(params ClipboardData[] data)
        {
            Forms.IDataObject dataObject = new Forms.DataObject();
            foreach (var clipboardData in data)
            {
                if (clipboardData.Data != null)
                    dataObject.SetData(Clipboard.FormatToString(clipboardData.Format), clipboardData.Data);
            }

            if (dataObject.GetFormats().Length > 1)
            {
                Forms.Clipboard.SetDataObject(dataObject, copy: true);
            }
        }

        internal static string FormatToString(ClipboardFormat format)
        {
            switch (format)
            {
                case ClipboardFormat.CommaSeparatedValues:
                    return Forms.DataFormats.CommaSeparatedValue;
                case ClipboardFormat.Html:
                    return Forms.DataFormats.Html;
                case ClipboardFormat.RichText:
                    return Forms.DataFormats.Rtf;
                case ClipboardFormat.XmlSpreadsheet:
                    return "Xml Spreadsheet";
                case ClipboardFormat.Text:
                default:
                    return Forms.DataFormats.Text;
            }
        }
    }
}
