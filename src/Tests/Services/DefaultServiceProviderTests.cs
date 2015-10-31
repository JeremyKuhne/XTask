// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Services
{
    using FluentAssertions;
    using XTask.Systems.Console;
    using XTask.Systems.File;
    using XTask.Services;
    using Xunit;

    public class DefaultServiceProviderTests
    {
        [Fact]
        public void ExpectedServicesFound()
        {
            FlexServiceProvider.Services.GetService<IFileService>().Should().NotBeNull();
            FlexServiceProvider.Services.GetService<IConsoleService>().Should().NotBeNull();
        }
    }
}
