// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using Forms = System.Windows.Forms;

    public static class Clipboard
    {
        /// <summary>
        /// Adds the specified data to the clipboard
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <param name="nameof(data)"/> is null.</exception>
        public static void AddToClipboard(params ClipboardData[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Forms.IDataObject dataObject = new Forms.DataObject();
            foreach (var clipboardData in data)
            {
                if (clipboardData.Data != null)
                    dataObject.SetData(GetDataObjectFormatString(clipboardData.Format), clipboardData.Data);
            }

            if (dataObject.GetFormats().Length > 1)
            {
                Forms.Clipboard.SetDataObject(dataObject, copy: true);
            }
        }

        /// <summary>
        /// Gets the proper set/getdata string for the given ClipboardFormat.
        /// </summary>
        internal static string GetDataObjectFormatString(ClipboardFormat format)
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
