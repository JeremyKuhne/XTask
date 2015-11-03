// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Services
{
    using Systems.Configuration;
    using Systems.Configuration.Concrete;
    using Systems.Console;
    using Systems.Console.Concrete;
    using Systems.File;
    using Systems.File.Concrete;
    using Systems.File.Concrete.Flex;

    /// <summary>
    /// Flexible services provider
    /// </summary>
    public static class FlexServiceProvider
    {
        private static SimpleServiceProvider concreteServices;

        static FlexServiceProvider()
        {
            concreteServices = new SimpleServiceProvider();
            var extendedFileService = new ExtendedFileService();
            concreteServices.AddService<IExtendedFileService>(extendedFileService);
            concreteServices.AddService<IFileService>(new FileService(extendedFileService));
            concreteServices.AddService<IConsoleService>(new ConcreteConsoleService());
            concreteServices.AddService<IConfigurationManager>(new ConfigurationManager());
        }

        public static ITypedServiceProvider Services
        {
            get
            {
                return FlexServiceProvider.concreteServices;
            }
        }
    }
}
