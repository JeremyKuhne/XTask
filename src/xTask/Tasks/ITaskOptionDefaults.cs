// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks;

/// <summary>
///  Get defaults for the given option, if any.
/// </summary>
public interface ITaskOptionDefaults
{
    T GetOptionDefault<T>(string option);
}
