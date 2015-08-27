// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Logging
{
    using Microsoft.Build.Framework;
    using System;

    /// <summary>
    /// Microsoft Build implementation of ILogger.
    /// </summary>
    public class BuildLogger : TextTableLogger
    {
        // Trying to be *very* careful to not use any types outside of Framework, particularly ones
        // from assemblies with versions in their names (such as Microsoft.Build.Utilities.v4.0.dll)
        // as you cannot retarget from one assembly name to another (v4.0.dll -> v12.0.dll).

        private string taskName;
        private bool hasLoggedErrors;
        private IBuildEngine buildEngine;

        public BuildLogger(IBuildEngine buildEngine, string taskName)
        {
            this.buildEngine = buildEngine;
            this.taskName = taskName;
        }

        // Consider adding this concept to ILogger to allow for easier build aborts
        public bool HasLoggedErrors { get { return this.hasLoggedErrors; } }

        protected override void WriteInternal(WriteStyle style, string value)
        {
            // TODO: How do we normalize this with the current design? Perhaps hold lines without a line end until we get one?

            // MSBuild ALWAYS is a "Writeline"
            value = value.Trim('\f', '\n', '\r');
            if (String.IsNullOrWhiteSpace(value)) { return; }

            if (style.HasFlag(WriteStyle.Error))
            {
                this.LogError(value);
            }
            else if (style.HasFlag(WriteStyle.Critical))
            {
                this.LogWarning(value);
            }
            else
            {
                MessageImportance importance = MessageImportance.Normal;
                if (style.HasFlag(WriteStyle.Bold) || style.HasFlag(WriteStyle.Important))
                {
                    importance = MessageImportance.High;
                }

                BuildMessageEventArgs message = new BuildMessageEventArgs(
                    message: value,
                    helpKeyword: null,
                    senderName: this.taskName,
                    importance: importance);

                this.buildEngine.LogMessageEvent(message);
            }
        }

        private void LogWarning(string value)
        {
            BuildWarningEventArgs warning = new BuildWarningEventArgs(
                subcategory: null,
                code: null,
                file: this.buildEngine.ProjectFileOfTaskNode,
                lineNumber: this.buildEngine.LineNumberOfTaskNode,
                columnNumber: this.buildEngine.ColumnNumberOfTaskNode,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: value,
                helpKeyword: null,
                senderName: this.taskName,
                eventTimestamp: DateTime.UtcNow);

            this.buildEngine.LogWarningEvent(warning);
        }

        private void LogError(string value)
        {
            BuildErrorEventArgs error = new BuildErrorEventArgs(
                subcategory: null,
                code: null,
                file: this.buildEngine.ProjectFileOfTaskNode,
                lineNumber: this.buildEngine.LineNumberOfTaskNode,
                columnNumber: this.buildEngine.ColumnNumberOfTaskNode,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: value,
                helpKeyword: null,
                senderName: this.taskName,
                eventTimestamp: DateTime.UtcNow);

            this.buildEngine.LogErrorEvent(error);
            this.hasLoggedErrors = true;
        }

        protected override int TableWidth
        {
            get { return 160; }
        }
    }
}