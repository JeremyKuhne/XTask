// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Text;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Ole;
using xTask.Utility;

namespace XTask.Logging
{
    public static class ClipboardHelper
    {
        /// <summary>
        ///  Adds the specified data to the clipboard
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <param name="nameof(data)"/> is null.</exception>
        public static void SetClipboardData(params ClipboardData[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            int retry = 4;
            while (!OpenClipboard())
            {
                if (--retry < 0)
                {
                    // Clipboard was locked.
                    return;
                }

                Thread.Sleep(200);
            }

            try
            {
                EmptyClipboard();
                foreach (var item in data)
                {
                    switch (item.Format)
                    {
                        case ClipboardFormat.UnicodeText:
                            SetClipboardUnicodeText(item.CharData);
                            break;
                        default:
                            ReadOnlySpan<char> charData = item.CharData;
                            if (charData.Length > 0)
                            {
                                SetClipboardAsciiText(charData, GetDataObjectFormatString(item.Format));
                            }
                            else
                            {
                                SetClipboardData(item.ByteData, GetDataObjectFormatString(item.Format));
                            }

                            break;
                    }
                }
            }
            finally
            {
                CloseClipboard();
            }
        }

        /// <summary>
        ///  Gets the proper set/getdata string for the given ClipboardFormat.
        /// </summary>
        private static string GetDataObjectFormatString(ClipboardFormat format) => format switch
        {
            ClipboardFormat.CommaSeparatedValues => "Csv",
            ClipboardFormat.Html => "HTML Format",
            ClipboardFormat.RichText => "Rich Text Format",
            ClipboardFormat.XmlSpreadsheet => "Xml Spreadsheet",
            ClipboardFormat.UnicodeText => "UnicodeText",
            _ => "Text",
        };

        private static bool OpenClipboard()
        {
            if (!Interop.OpenClipboard(default))
            {
                WIN32_ERROR error = Error.GetLastError();
                if (error == WIN32_ERROR.ERROR_ACCESS_DENIED)
                {
                    // Clipboard is already open.
                    return false;
                }

                error.Throw();
            }

            return true;
        }

        private static void EmptyClipboard() => Interop.EmptyClipboard().ThrowLastErrorIfFalse();

        private static void CloseClipboard()
        {
            if (!Interop.CloseClipboard())
            {
                WIN32_ERROR error = Error.GetLastError();
                if (error == WIN32_ERROR.ERROR_CLIPBOARD_NOT_OPEN)
                {
                    // Clipboard isn't open. We won't throw here as it is pretty common to fail to open the
                    // clipboard (as some other window may have it open).
                    return;
                }

                error.Throw();
            }
        }

        /// <summary>
        ///  Set Unicode text in the clipboard under the given format.
        /// </summary>
        public static unsafe void SetClipboardUnicodeText(
            ReadOnlySpan<char> span,
            uint format = (uint)CLIPBOARD_FORMAT.CF_UNICODETEXT)
            => SetClipboardData(span, format);

        /// <summary>
        ///  Set ASCII text in the clipboard under the given format.
        /// </summary>
        public static unsafe void SetClipboardAsciiText(
            ReadOnlySpan<char> span,
            uint format = (uint)CLIPBOARD_FORMAT.CF_TEXT)
        {
            Encoding ascii = Encoding.ASCII;
            fixed (char* c = span)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(ascii.GetByteCount(c, span.Length));
                fixed (byte* b = buffer)
                {
                    int length = ascii.GetBytes(c, span.Length, b, buffer.Length);
                    SetClipboardData<byte>(buffer.AsSpan(0, length), format);
                }

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        ///  Set ASCII text in the clipboard under the given format.
        /// </summary>
        public static unsafe void SetClipboardAsciiText(ReadOnlySpan<char> span, string format)
            => SetClipboardAsciiText(span, RegisterClipboardFormat(format));

        /// <summary>
        ///  Sets data in the clipboard under the given format.
        /// </summary>
        public static unsafe void SetClipboardData<T>(ReadOnlySpan<T> data, uint format)
            where T : unmanaged
        {
            HGLOBAL global = Interop.GlobalAlloc(
                GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE,
                (nuint)((data.Length + 1) * sizeof(T)));

            Span<T> buffer = new(Interop.GlobalLock(global), data.Length + 1);
            data.CopyTo(buffer);
            buffer[buffer.Length - 1] = default;

            Interop.GlobalUnlock(global);
            Interop.SetClipboardData(format, (HANDLE)(nint)global);
        }

        /// <summary>
        ///  Sets binary data in the clipboard under the given format.
        /// </summary>
        public static void SetClipboardData<T>(ReadOnlySpan<T> data, string format) where T : unmanaged
            => SetClipboardData(data, RegisterClipboardFormat(format));

        /// <summary>
        ///  Registers the given format if not already registered. Returns the format id.
        /// </summary>
        public static uint RegisterClipboardFormat(string format)
        {
            uint id = Interop.RegisterClipboardFormat(format);
            if (id == 0)
            {
                Error.ThrowLastError(format);
            }

            return id;
        }
    }
}
