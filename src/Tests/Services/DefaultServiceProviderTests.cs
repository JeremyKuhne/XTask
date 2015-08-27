// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Services
{
    using FluentAssertions;
    using XTask.ConsoleSystem;
    using XTask.FileSystem;
    using XTask.Services;
    using Xunit;

    public class DefaultServiceProviderTests
    {
        [Fact]
        public void ExpectedServicesFound()
        {
            DefaultServiceProvider.Services.GetService<IFileService>().Should().NotBeNull();
            DefaultServiceProvider.Services.GetService<IConsoleService>().Should().NotBeNull();
        }
    }
}
