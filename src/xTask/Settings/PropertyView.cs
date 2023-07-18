// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace XTask.Settings;

/// <summary>
///  Stub for building objects that can be translated on output.
/// </summary>
public abstract class PropertyView : IPropertyView
{
    public abstract IEnumerator<IProperty<object>> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        var typedEnumerator = GetEnumerator();
        while (typedEnumerator.MoveNext())
        {
            yield return typedEnumerator.Current;
        }
    }
}