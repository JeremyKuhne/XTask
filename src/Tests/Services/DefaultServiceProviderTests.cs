// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Systems.Console;
using XTask.Systems.File;
using XTask.Services;

namespace XTask.Tests.Services;

public class DefaultServiceProviderTests
{
    [Fact]
    public void ExpectedServicesFound()
    {
        FlexServiceProvider.Services.GetService<IFileService>().Should().NotBeNull();
        FlexServiceProvider.Services.GetService<IConsoleService>().Should().NotBeNull();
    }
}
