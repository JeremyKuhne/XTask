// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Utility;
using System.Text;
using System;

namespace XTask.Logging
{
    public class TextLogger : TextTableLogger, IClipboardSource
    {
        private readonly StringBuilder _text = new(4096);

        protected override int TableWidth => 120;

        protected override void WriteInternal(WriteStyle style, string value)
            => _text.Append(style.HasFlag(WriteStyle.Underline) ? Strings.Underline(value) : value);

        public override string ToString() => _text.ToString();

        public ClipboardData GetClipboardData()
            => _text.Length == 0 ? default : new(ToString().AsMemory(), ClipboardFormat.UnicodeText);
    }
}
