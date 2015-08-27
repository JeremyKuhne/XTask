// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Services
{
    /// <summary>
    /// Simple service provider that simply checks for implemenation of the given type.
    /// Normal usage is to find derived classes interface implementations. This allows
    /// creation of wrapper classes that can defer to this class for interfaces they
    /// do not know about.
    /// </summary>
    public abstract class ImplementedServiceProvider : ITypedServiceProvider
    {
        public virtual T GetService<T>() where T : class
        {
            return this as T;
        }
    }
}
