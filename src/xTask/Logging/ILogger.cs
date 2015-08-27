// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    public interface ILogger
    {
        void Write(string value);
        void Write(string format, params object[] args);
        void Write(WriteStyle style, string format, params object[] args);
        void Write(WriteStyle style, string value);
        void Write(ITable table);
        void WriteLine();
        void WriteLine(string value);
        void WriteLine(string format, params object[] args);
        void WriteLine(WriteStyle style, string format, params object[] args);
        void WriteLine(WriteStyle style, string value);
    }
}
