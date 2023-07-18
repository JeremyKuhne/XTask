// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Framework;
using System;

namespace XTask.Logging;

/// <summary>
///  Microsoft Build implementation of ILogger.
/// </summary>
public class BuildLogger : TextTableLogger
{
    // Trying to be *very* careful to not use any types outside of Framework, particularly ones
    // from assemblies with versions in their names (such as Microsoft.Build.Utilities.v4.0.dll)
    // as you cannot retarget from one assembly name to another (v4.0.dll -> v12.0.dll).

    private readonly string _taskName;
    private bool _hasLoggedErrors;
    private readonly IBuildEngine _buildEngine;

    public BuildLogger(IBuildEngine buildEngine, string taskName)
    {
        _buildEngine = buildEngine;
        _taskName = taskName;
    }

    // Consider adding this concept to ILogger to allow for easier build aborts
    public bool HasLoggedErrors { get { return _hasLoggedErrors; } }

    protected override void WriteInternal(WriteStyle style, string value)
    {
        // TODO: How do we normalize this with the current design? Perhaps hold lines without a line end until we get one?

        // MSBuild ALWAYS is a "Writeline"
        value = value.Trim('\f', '\n', '\r');
        if (string.IsNullOrWhiteSpace(value)) { return; }

        if (style.HasFlag(WriteStyle.Error))
        {
            LogError(value);
        }
        else if (style.HasFlag(WriteStyle.Critical))
        {
            LogWarning(value);
        }
        else
        {
            MessageImportance importance = MessageImportance.Normal;
            if (style.HasFlag(WriteStyle.Bold) || style.HasFlag(WriteStyle.Important))
            {
                importance = MessageImportance.High;
            }

            BuildMessageEventArgs message = new(
                message: value,
                helpKeyword: null,
                senderName: _taskName,
                importance: importance);

            _buildEngine.LogMessageEvent(message);
        }
    }

    private void LogWarning(string value)
    {
        BuildWarningEventArgs warning = new(
            subcategory: null,
            code: null,
            file: _buildEngine.ProjectFileOfTaskNode,
            lineNumber: _buildEngine.LineNumberOfTaskNode,
            columnNumber: _buildEngine.ColumnNumberOfTaskNode,
            endLineNumber: 0,
            endColumnNumber: 0,
            message: value,
            helpKeyword: null,
            senderName: _taskName,
            eventTimestamp: DateTime.UtcNow);

        _buildEngine.LogWarningEvent(warning);
    }

    private void LogError(string value)
    {
        BuildErrorEventArgs error = new(
            subcategory: null,
            code: null,
            file: _buildEngine.ProjectFileOfTaskNode,
            lineNumber: _buildEngine.LineNumberOfTaskNode,
            columnNumber: _buildEngine.ColumnNumberOfTaskNode,
            endLineNumber: 0,
            endColumnNumber: 0,
            message: value,
            helpKeyword: null,
            senderName: _taskName,
            eventTimestamp: DateTime.UtcNow);

        _buildEngine.LogErrorEvent(error);
        _hasLoggedErrors = true;
    }

    protected override int TableWidth => 160;
}