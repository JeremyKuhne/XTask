// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings;

/// <summary>
///  Simple interface for a named value pair
/// </summary>
public interface IProperty<out T>
{
    string Name { get; }
    T Value { get; }
}