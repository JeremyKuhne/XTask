// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace XTask.Services;

public class SimpleServiceProvider : ITypedServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public void AddService<T>(T service) where T : class => _services.Add(typeof(T), service);

    public T GetService<T>() where T : class
    {
        _services.TryGetValue(typeof(T), out object value);
        return value as T;
    }
}
