// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    /// <summary>
    ///  Provides a registry for property views for objects.
    /// </summary>
    public interface IPropertyViewProvider
    {
        /// <summary>
        ///  Register a constructor for an IPropertyView wrapper class.
        /// </summary>
        void RegisterPropertyViewer<T>(PropertyViewConstructor<T> propertyViewer);

        /// <summary>
        ///  Get a view for the given object.
        /// </summary>
        IPropertyView GetTypeView<T>(T value);
    }
}