// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Services
{
    using System;
    using System.Collections.Generic;
    using XTask.Systems.Console;
    using XTask.Systems.Console.Concrete;
    using XTask.Systems.File;
    using XTask.Systems.File.Concrete.Flex;

    public static class DefaultServiceProvider
    {
        private static ITypedServiceProvider concreteServices;

        static DefaultServiceProvider()
        {
            DefaultServiceProvider.concreteServices = new ConcreteServices();
        }

        public static ITypedServiceProvider Services
        {
            get
            {
                return DefaultServiceProvider.concreteServices;
            }
        }

        private class ConcreteServices : ITypedServiceProvider
        {
            private Dictionary<Type, object> concreteServices = new Dictionary<Type, object>();

            public ConcreteServices()
            {
                var fileService = new FileService();
                this.concreteServices.Add(typeof(IFileService), fileService);
                this.concreteServices.Add(typeof(IExtendedFileService), fileService);
                this.concreteServices.Add(typeof(IConsoleService), new ConcreteConsoleService());
            }

            public T GetService<T>() where T : class
            {
                object value;
                this.concreteServices.TryGetValue(typeof(T), out value);
                return value as T;
            }
        }
    }
}
