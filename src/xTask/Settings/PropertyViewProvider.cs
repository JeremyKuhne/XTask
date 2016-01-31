// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Basic implementation of a property view provider. Allows registering property view wrappers
    /// for types that don't implement IPropertyView. Also has a basic ToString default view.
    /// </summary>
    public class PropertyViewProvider : IPropertyViewProvider
    {
        private Dictionary<Type, object> _propertyViewers = new Dictionary<Type, object>();

        public void RegisterPropertyViewer<T>(PropertyViewConstructor<T> propertyViewer)
        {
            _propertyViewers.Add(typeof(T), propertyViewer);
        }

        public IPropertyView GetTypeView<T>(T value)
        {
            // Look for built-in type view support, registered type view, then fall back on default
            IPropertyView typeView = value as IPropertyView;
            if (typeView != null) return typeView;

            object propertyViewer;
            Type valueType = value.GetType();
            if (_propertyViewers.TryGetValue(valueType, out propertyViewer))
            {
                Delegate constructor = (Delegate)propertyViewer;
                return (IPropertyView)constructor.DynamicInvoke(value);
            }

            return DefaultTypeView.Create(value);
        }
    }
}