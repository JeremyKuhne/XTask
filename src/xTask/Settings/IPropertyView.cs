// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System.Collections.Generic;

    public delegate IPropertyView PropertyViewConstructor<T>(T value);

    /// <summary>
    /// Specifies the class has an enumerable set of properties
    /// </summary>
    /// <remarks>
    /// Implementing this allows output providers to translate objects (MSBuild for one example)
    /// </remarks>
    public interface IPropertyView : IEnumerable<IProperty<object>>
    {
    }
}