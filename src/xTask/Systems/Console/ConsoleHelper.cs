// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.Console
{
    using System;
    using System.Globalization;
    using XTask.Services;

    public static class ConsoleHelper
    {
        /// <summary>
        /// Access to the default Console
        /// </summary>
        public static IConsoleService Console
        {
            get { return FlexServiceProvider.Services.GetService<IConsoleService>(); }
        }

        /// <summary>
        /// Does a locked write to the console in the specified foreground color, resetting when finished.
        /// </summary>
        public static void WriteLockedColor(this IConsoleService console, ConsoleColor color, string value)
        {
            try
            {
                console.ForegroundColor = color;
                console.Write(value);
            }
            finally
            {
                console.ResetColor();
            }
        }

        /// <summary>
        /// Allows scoping an appended status onto the Console title bar. Use in Using statement.
        /// </summary>
        public static IDisposable AppendConsoleTitle(this IConsoleService console, string value)
        {
            return new StatusAppender(console, value);
        }

        /// <summary>
        /// Allows scoping an appended status onto the Console title bar. Use in Using statement.
        /// </summary>
        public static IDisposable AppendConsoleTitle(this IConsoleService console, string format, params object[] args)
        {
            return new StatusAppender(console, string.Format(CultureInfo.CurrentUICulture, format, args));
        }

        private class StatusAppender : IDisposable
        {
            private string _originalTitle;
            private IConsoleService _console;

            public StatusAppender(IConsoleService console, string status)
            {
                _console = console;
                _originalTitle = _console.Title;
                _console.Title = string.Format(CultureInfo.CurrentUICulture, "{0}: {1}", _originalTitle, status);
            }

            public void Dispose()
            {
                _console.Title = _originalTitle;
            }
        }

        /// <summary>
        /// Get a yes/no answer from the console.
        /// </summary>
        public static bool QueryYesNo(this IConsoleService console, string format, params object[] args)
        {
            return console.QueryYesNo(string.Format(CultureInfo.CurrentUICulture, format, args));
        }

        /// <summary>
        /// Get a yes/no answer from the console.
        /// </summary>
        public static bool QueryYesNo(this IConsoleService console, string value)
        {
            string queryString = string.Format(CultureInfo.CurrentUICulture, XTaskStrings.YesNoQueryStringFormat, value) + "\n";
            console.WriteLockedColor(console.ForegroundColor, queryString);
            string answer = console.ReadLine().Trim();
            return string.Equals(answer, XTaskStrings.YesResponse, StringComparison.CurrentCultureIgnoreCase)
                || string.Equals(answer, XTaskStrings.YesShortResponse, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
