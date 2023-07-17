// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Services
{
    /// <summary>
    ///  A stronger typed IServiceProvider.
    /// </summary>
    public interface ITypedServiceProvider
    {
        /// <summary>
        ///  Gets the service of the specified type.
        /// </summary>
        T GetService<T>() where T : class;
    }
}
