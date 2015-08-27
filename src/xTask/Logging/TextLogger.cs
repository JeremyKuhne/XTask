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
        private StringBuilder text = new StringBuilder(4096);

        protected override int TableWidth
        {
            get { return 120; }
        }

        protected override void WriteInternal(WriteStyle style, string value)
        {
            if (style.HasFlag(WriteStyle.Underline))
            {
                text.Append(Strings.Underline(value));
            }
            else
            {
                text.Append(value);
            }
        }

        public override string ToString()
        {
            return this.text.ToString();
        }

        public ClipboardData GetClipboardData()
        {
            return new ClipboardData { Data = text.Length > 0 ? this.ToString() : null, Format = ClipboardFormat.Text };
        }
    }
}
