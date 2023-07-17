// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;


namespace XTask.Settings
{
    public class DefaultTypeView : PropertyView
    {
        private DefaultTypeView(object value) => Value = value;

        public object Value { get; private set; }

        public static IPropertyView Create(object value) => new DefaultTypeView(value);

        public override IEnumerator<IProperty<object>> GetEnumerator()
        {
            // No properties to give for something we don't understand
            yield break;
        }

        public override string ToString() => Value.ToString();
    }
}