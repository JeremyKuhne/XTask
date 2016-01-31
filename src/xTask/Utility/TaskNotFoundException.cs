// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Used to centrally handle missing items
    /// </summary>
    [Serializable]
    public class TaskNotFoundException : TaskException
    {
        public TaskNotFoundException(string item)
            : base(string.Format(CultureInfo.CurrentUICulture, XTaskStrings.CouldNotFindGeneral, item))
        {
        }

        public TaskNotFoundException(string item, string detailMessage)
            : base(string.Format(CultureInfo.CurrentUICulture, XTaskStrings.CouldNotFindGeneral, item, detailMessage))
        {
        }

        public override ExitCode ExitCode
        {
            get { return ExitCode.PathNotFound; }
        }
    }
}
