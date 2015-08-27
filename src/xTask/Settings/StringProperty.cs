// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;

    /// <summary>
    /// Simple Tuple compatible IProperty implementation for strings
    /// </summary>
    public class StringProperty : Tuple<string, string>, IStringProperty
    {
        public StringProperty(string name, string value)
            : base(name, value)
        {
        }

        public string Name
        {
            get { return this.Item1; }
        }

        public string Value
        {
            get { return this.Item2; }
        }
    }
}