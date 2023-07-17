// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Systems.Configuration;
using XTask.Systems.Configuration.Concrete;
using XTask.Systems.Console;
using XTask.Systems.Console.Concrete;
using XTask.Systems.File;
using XTask.Systems.File.Concrete;
using XTask.Systems.File.Concrete.Flex;

namespace XTask.Services
{
    /// <summary>
    ///  Flexible services provider.
    /// </summary>
    public static class FlexServiceProvider
    {
        private static readonly SimpleServiceProvider _concreteServices;

        static FlexServiceProvider()
        {
            _concreteServices = new SimpleServiceProvider();
            var extendedFileService = new ExtendedFileService();
            _concreteServices.AddService<IExtendedFileService>(extendedFileService);
            _concreteServices.AddService<IFileService>(new FileService(extendedFileService));
            _concreteServices.AddService<IConsoleService>(new ConcreteConsoleService());
            _concreteServices.AddService<IConfigurationManager>(new ConfigurationManager());
        }

        public static ITypedServiceProvider Services
        {
            get
            {
                return _concreteServices;
            }
        }
    }
}
