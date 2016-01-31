// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;

    /// <summary>
    /// Simple Tuple compatible IProperty implementation
    /// </summary>
    public class Property : Tuple<string, object>, IProperty<object>
    {
        public Property(string name, string value)
            : base(name, value)
        {
        }

        public string Name
        {
            get { return Item1; }
        }

        public object Value
        {
            get { return Item2; }
        }
    }
}