// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using XTask.Utility;
    using System.Text;

    public class TextLogger : TextTableLogger, IClipboardSource
    {
        private StringBuilder _text = new StringBuilder(4096);

        protected override int TableWidth
        {
            get { return 120; }
        }

        protected override void WriteInternal(WriteStyle style, string value)
        {
            if (style.HasFlag(WriteStyle.Underline))
            {
                _text.Append(Strings.Underline(value));
            }
            else
            {
                _text.Append(value);
            }
        }

        public override string ToString()
        {
            return _text.ToString();
        }

        public ClipboardData GetClipboardData()
        {
            return new ClipboardData { Data = _text.Length > 0 ? ToString() : null, Format = ClipboardFormat.Text };
        }
    }
}
