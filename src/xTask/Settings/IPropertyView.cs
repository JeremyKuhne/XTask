// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace XTask.Settings
{
    public delegate IPropertyView PropertyViewConstructor<T>(T value);

    /// <summary>
    ///  Specifies the class has an enumerable set of properties
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Implementing this allows output providers to translate objects (MSBuild for one example)
    ///  </para>
    /// </remarks>
    public interface IPropertyView : IEnumerable<IProperty<object>>
    {
    }
}