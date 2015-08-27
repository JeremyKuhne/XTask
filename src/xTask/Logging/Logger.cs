// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using System;
    using System.Globalization;

    public abstract class Logger : ILogger
    {
        private const string NewLine = "\r\n";

        public void Write(string value)
        {
            this.Write(WriteStyle.Current, value);
        }

        public void Write(string format, params object[] args)
        {
            this.Write(WriteStyle.Current, format, args);
        }

        public void Write(WriteStyle style, string format, params object[] args)
        {
            this.Write(style, String.Format(CultureInfo.CurrentUICulture, format, args));
        }

        public void Write(WriteStyle style, string value)
        {
            if (style.HasFlag(WriteStyle.Error))
            {
                this.WriteInternal(style, String.Format(CultureInfo.CurrentUICulture, XTaskStrings.ErrorFormatString, value));
            }
            else
            {
                this.WriteInternal(style, value);
            }
        }

        protected abstract void WriteInternal(WriteStyle style, string value);

        public void WriteLine()
        {
            this.Write(Logger.NewLine);
        }

        public void WriteLine(string value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public void WriteLine(string format, params object[] args)
        {
            this.Write(format, args);
            this.WriteLine();
        }

        public void WriteLine(WriteStyle style, string format, params object[] args)
        {
            this.Write(style, format, args);
            this.WriteLine();
        }

        public void WriteLine(WriteStyle style, string value)
        {
            this.Write(style, value);
            this.WriteLine();
        }

        public abstract void Write(ITable table);
    }
}