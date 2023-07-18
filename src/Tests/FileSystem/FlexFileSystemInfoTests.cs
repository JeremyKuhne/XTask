// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using XTask.Systems.File;
using XTask.Services;

namespace XTask.Tests.FileSystem;

public class FlexFileSystemInfoTests
{
    [Theory,
        InlineData(""),
        InlineData(Paths.ExtendedPathPrefix)]
    public void CreateInfoForRootDrive(string prefix)
    {
        string driveRoot = prefix + Paths.GetRoot(Path.GetTempPath());
        IFileService fileService = FlexServiceProvider.Services.GetService<IFileService>();

        var info = fileService.GetPathInfo(driveRoot);
        info.Should().BeAssignableTo<IDirectoryInformation>();
        info.Exists.Should().BeTrue();
        info.Name.Should().Be(driveRoot);
        info.Path.Should().Be(driveRoot);
    }

    [Theory,
        InlineData(@"\\.\pipe\"),
        InlineData(@"\\?\pipe\")

        // Currently these throw as many file apis don't like the file handle that this creates- still figuring out the best way to handle it
        //InlineData(@"\\.\pipe")
        //InlineData(@"\\?\pipe")
        ]
    public void CreateInfoForPipeRoot(string path)
    {
        IFileService fileService = FlexServiceProvider.Services.GetService<IFileService>();

        var info = fileService.GetPathInfo(path);
        info.Should().BeAssignableTo<IDirectoryInformation>();
    }
}
