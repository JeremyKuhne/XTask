// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks;

// Trying to follow standard windows error codes here
public enum ExitCode : int
{
    Success = 0,
    GeneralFailure = 1,
    FileNotFound = 2,
    PathNotFound = 3,
    AccessDenied = 5,
    InvalidData = 13,
    NetworkPathNotFound = 53,
    Canceled = 1223,
    NetworkConnectionFailed = 2250,
    InvalidArgument = 10022
}