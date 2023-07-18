// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings;

/// <summary>
///  Basic implementation of a property view provider. Allows registering property view wrappers
///  for types that don't implement IPropertyView. Also has a basic ToString default view.
/// </summary>
public class PropertyViewProvider : IPropertyViewProvider
{
    private readonly Dictionary<Type, object> _propertyViewers = new();

    public void RegisterPropertyViewer<T>(PropertyViewConstructor<T> propertyViewer)
    {
        _propertyViewers.Add(typeof(T), propertyViewer);
    }

    public IPropertyView GetTypeView<T>(T value)
    {
        // Look for built-in type view support, registered type view, then fall back on default.
        if (value is IPropertyView typeView)
        {
            return typeView;
        }

        Type valueType = value.GetType();
        if (_propertyViewers.TryGetValue(valueType, out object propertyViewer))
        {
            Delegate constructor = (Delegate)propertyViewer;
            return (IPropertyView)constructor.DynamicInvoke(value);
        }

        return DefaultTypeView.Create(value);
    }
}