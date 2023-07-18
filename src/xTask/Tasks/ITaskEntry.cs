// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks;

public interface ITaskEntry
{
    IEnumerable<string> Aliases { get; }
    ITask Task { get; }
}
