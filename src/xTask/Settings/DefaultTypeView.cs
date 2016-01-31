// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System.Collections.Generic;

    public class DefaultTypeView : PropertyView
    {
        private DefaultTypeView(object value)
        {
            this.Value = value;
        }

        public object Value { get; private set; }

        public static IPropertyView Create(object value)
        {
            return new DefaultTypeView(value);
        }

        public override IEnumerator<IProperty<object>> GetEnumerator()
        {
            // No properties to give for something we don't understand
            yield break;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}