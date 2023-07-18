// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Settings;

/// <summary>
///  Simple <see cref="Tuple"/> compatible <see cref="IProperty{String}"/> implementation for strings.
/// </summary>
public class StringProperty : Tuple<string, string>, IStringProperty
{
    public StringProperty(string name, string value)
        : base(name, value)
    {
    }

    public string Name => Item1;

    public string Value => Item2;
}