// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System;
using System.Text;
using XTask.Systems.File;
using Xunit;

namespace XTask.Tests.FileSystem;

public class PathsTests
{
    [Theory,
        InlineData("", true),
        InlineData("C:", true),
        InlineData("**", true),
        InlineData(@"\\.\path", false),
        InlineData(@"\\?\path", false),
        InlineData(@"\??\path", false),
        InlineData(@"\\.", false),
        InlineData(@"\\?", false),
        InlineData(@"\\", false),
        InlineData(@"//", false),
        InlineData(@"\?", false),
        InlineData(@"/?", false),
        InlineData(@"\", true),
        InlineData(@"/", true),
        InlineData(@"C:Path", true),
        InlineData(@"C:\Path", false),
        InlineData(@"\\?\C:\Path", false),
        InlineData(@"Path", true),
        InlineData(@"X", true)
        ]
    public void IsPathPartiallyQualified(string path, bool expected)
    {
        Paths.IsPartiallyQualified(path).Should().Be(expected);
    }

    [Theory,
        InlineData(null, PathFormat.UnknownFormat),
        InlineData(@" ", PathFormat.LocalCurrentDirectoryRelative),
        InlineData(@"\\?\UNC\a\ ", PathFormat.UniformNamingConvention),
        InlineData(@"\\a\ ", PathFormat.UniformNamingConvention),
        InlineData(@"\\.\ ", PathFormat.LocalFullyQualified),
        InlineData(@"", PathFormat.UnknownFormat),
        InlineData(@"\\?\UNC\a\", PathFormat.UnknownFormat),
        InlineData(@"\\a\", PathFormat.UnknownFormat),
        InlineData(@"\\.\", PathFormat.UnknownFormat)]
    public void PathFormatChangesWithTrailingSpace(string path, PathFormat expected)
    {
        Paths.GetPathFormat(path).Should().Be(expected, "Passed path was '{0}'", path);
    }

    [Theory,
        InlineData(@"C:", PathFormat.LocalDriveRooted),
        InlineData(@"C:Foo", PathFormat.LocalDriveRooted),
        InlineData(@"C:\", PathFormat.LocalFullyQualified),
        InlineData(@"C:\Foo", PathFormat.LocalFullyQualified),
        InlineData(@"C", PathFormat.LocalCurrentDirectoryRelative),
        InlineData(@"@:", PathFormat.UnknownFormat),
        InlineData(@"[:", PathFormat.UnknownFormat),
        InlineData(@"Foo", PathFormat.LocalCurrentDirectoryRelative),
        InlineData(@"\", PathFormat.LocalCurrentDriveRooted),
        InlineData(@"\Foo", PathFormat.LocalCurrentDriveRooted),
        InlineData(@"/", PathFormat.LocalCurrentDriveRooted),
        InlineData(@"/\", PathFormat.UnknownFormat),
        InlineData(@"\\Foo\Bar", PathFormat.UniformNamingConvention),
        InlineData(@"\\Foo\Bar\", PathFormat.UniformNamingConvention),
        InlineData(@"\\?\\Foo\Bar", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\Foo\Bar", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\Foo\Bar\", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\UNC\\Foo\Bar", PathFormat.UnknownFormat),
        InlineData(@"\\?\UNC\\", PathFormat.UnknownFormat),
        InlineData(@"\\?\UNC\a\\", PathFormat.UnknownFormat),
        InlineData(@"\\?\UNC\a\b", PathFormat.UniformNamingConvention),
        InlineData(@"\\?\UNC\Foo\Bar", PathFormat.UniformNamingConvention),
        InlineData(@"\\?\UNC\Foo\Bar\", PathFormat.UniformNamingConvention),
        InlineData(@":", PathFormat.UnknownFormat),
        InlineData(@"\\?\C:", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\@:\", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\[:\", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\C:\", PathFormat.LocalFullyQualified),
        InlineData(@"\\?\C:\Foo", PathFormat.LocalFullyQualified),
        InlineData(@"\\.psf\Home\", PathFormat.UniformNamingConvention),
        InlineData(@"\\.", PathFormat.UnknownFormat),
        InlineData(@"\\a\b", PathFormat.UniformNamingConvention),
        InlineData(@"\\\", PathFormat.UnknownFormat),
        InlineData(@"\\a\\", PathFormat.UnknownFormat)]
    public void PathFormatIsTheSameWithTrailingSpace(string path, PathFormat expected)
    {
        Paths.GetPathFormat(path).Should().Be(expected, "Passed path was '{0}'", path);
        Paths.GetPathFormat(path + " ").Should().Be(expected, "Passed path was '{0}'", path);
    }

    [Theory,
        InlineData(null, null),
        InlineData(@"C:", @"C:"),
        InlineData(@"C:Foo", @"C:"),
        InlineData(@"C:\", @"C:\"),
        InlineData(@"C:\Foo", @"C:\"),
        InlineData(@"C", ""),
        InlineData(@"@:", null),
        InlineData(@"[:", null),
        InlineData(@"Foo", ""),
        InlineData(@"\", @"\"),
        InlineData(@"\Foo", @"\"),
        InlineData(@"/", @"/"),
        InlineData(@"/\", null),
        InlineData(@"\\Foo\Bar", @"\\Foo\Bar"),
        InlineData(@"\\Foo\Bar\", @"\\Foo\Bar\"),
        InlineData(@"\\Foo\Bar\Foo", @"\\Foo\Bar\"),
        InlineData(@"\\Foo\Bar\Foo.txt", @"\\Foo\Bar\"),
        InlineData(@"\\?\\Foo\Bar", @"\\?\\Foo\"),
        InlineData(@"\\?\Foo\Bar", @"\\?\Foo\"),
        InlineData(@"\\?\Foo\Bar\", @"\\?\Foo\"),
        InlineData(@"\\?\GLOBALROOT\GLOBAL??\C:\Foo", @"\\?\GLOBALROOT\GLOBAL??\C:\"),
        InlineData(@"\\?\UNC\\Foo\Bar", null),
        InlineData(@"\\?\UNC\\", null),
        InlineData(@"\\?\UNC\a\", null),
        InlineData(@"\\?\UNC\a\\", null),
        InlineData(@"\\?\UNC\a\b", @"\\?\UNC\a\b"),
        InlineData(@"\\?\UNC\Foo\Bar", @"\\?\UNC\Foo\Bar"),
        InlineData(@"\\?\UNC\Foo\Bar\", @"\\?\UNC\Foo\Bar\"),
        InlineData(@"", null),
        InlineData(@":", null),
        InlineData(@"\\?\C:", @"\\?\C:"),
        InlineData(@"\\?\@:\", @"\\?\@:\"),
        InlineData(@"\\?\[:\", @"\\?\[:\"),
        InlineData(@"\\?\C:\", @"\\?\C:\"),
        InlineData(@"\\?\C:\Foo", @"\\?\C:\"),
        InlineData(@"\\?\\C:\\", @"\\?\\C:\"),
        InlineData(@"\\.psf\Home\", @"\\.psf\Home\"),
        InlineData(@"\\.", null),
        InlineData(@"\\a\b", @"\\a\b"),
        InlineData(@"\\\", null),
        InlineData(@"\\a\", null),
        InlineData(@"\\a\\", null),
        InlineData(@"\\.\", null)
        ]
    public void GetPathRoot(string path, string expected)
    {
        Paths.GetRoot(path).Should().Be(expected, "Passed path was {0}", path);
    }

    [Theory,
        InlineData(null, null),
        InlineData(@"C:\", @"C:\"),
        InlineData(@"C:\Foo", @"C:\"),
        InlineData(@"C:\Foo\", @"C:\Foo\"),
        InlineData(@"C:\Foo\Foo.txt", @"C:\Foo\"),
        InlineData(@"\\LocalHost\Share", @"\\LocalHost\Share\"),
        InlineData(@"\\LocalHost\Share\", @"\\LocalHost\Share\"),
        InlineData(@"\\LocalHost\Share\Foo", @"\\LocalHost\Share\"),
        InlineData(@"\\LocalHost\Share\Foo.txt", @"\\LocalHost\Share\")]
    public void GetDirectory(string path, string expected)
    {
        Paths.GetDirectory(path).Should().Be(expected, "Passed path was {0}", path);
    }

    [Theory,
        InlineData(null, null),
        InlineData(@"C:\", null),
        InlineData(@"C:\Foo", @"Foo"),
        InlineData(@"C:\Foo.txt", @"Foo.txt"),
        InlineData(@"C:\Foo\", @"Foo"),
        InlineData(@"C:\Foo\Foo.txt", @"Foo.txt"),
        InlineData(@"\\", null),
        InlineData(@"\\?\\C:\\", null),
        InlineData(@"\\LocalHost\Share", null),
        InlineData(@"\\LocalHost\Share\", null),
        InlineData(@"\\LocalHost\Share\Foo", @"Foo"),
        InlineData(@"\\LocalHost\Share\Foo.txt", @"Foo.txt")]
    public void GetFileOrDirectoryName(string path, string expected)
    {
        Paths.GetFileOrDirectoryName(path).Should().Be(expected, "Passed path was {0}", path);
    }

    [Theory,
        InlineData(null, @""),
        InlineData(@"", @""),
        InlineData(@".", @""),
        InlineData(@"FoooF", @""),
        InlineData(@"C:", @""),
        InlineData(@"C:\", @""),
        InlineData(@"C:\Foo", @""),
        InlineData(@"Foo.txt.txt", @".txt"),
        InlineData(@"Foo.txt.", @""),
        InlineData(@"C:\Foo.bar\Foo", @"")]
    public void GetExtension(string path, string expected)
    {
        Paths.GetExtension(path).Should().Be(expected, "Passed path was {0}", path);
    }

    [Theory,
        InlineData(null, new string[0]),
        InlineData(new[] { "" }, new string[0]),
        InlineData(new[] { @"C:\Foo\Bar\", @"C:\Foo\" }, new[] { @"C:\Foo\" }),
        InlineData(new[] { @"C:\", @"D:\Foo\Bar.txt" }, new[] { @"C:\", @"D:\Foo\" }),
        InlineData(new[] { @"C:\Bar\Bar.txt", @"C:\Foo\Car", @"C:\Foo\Bar.txt", @"\\LocalHost\Share", @"\\LocalHost\Share\", @"C:\Bar\", @"\\LocalHost\Share\Foo\Bar.txt", @"C:\Foo\Bar\Foo.txt", },
                    new[] { @"C:\Bar\", @"C:\Foo\", @"\\LocalHost\Share\" }),
        InlineData(new[] { @"C:\A\B\C.txt", @"C:\A\B\C\D.txt", @"C:\B\A", @"C:\B\A\", @"C:\C\A\B\A.txt", @"C:\C\A\B\C\C.txt", @"C:\A\B\C.txt", @"C:\D\A.txt" },
                    new[] { @"C:\A\B\", @"C:\B\", @"C:\C\A\B\", @"C:\D\" }),
        InlineData(new[] { @"C:\A\a", @"C:\A\b", @"C:\A\b", @"C:\B\a" },
                    new[] { @"C:\A\", @"C:\B\" })
        ]
    public void FindCommonPathRoots(string[] paths, string[] expected)
    {
        Paths.FindCommonRoots(paths).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void CombineThrowsOnNull()
    {
        Action action = () => Paths.Combine((string)null, null);
        action.Should().Throw<ArgumentNullException>("null string should throw");

        action = () => Paths.Combine((StringBuilder)null, null);
        action.Should().Throw<ArgumentNullException>("null stringbuilder should throw");
    }

    [Theory,
        InlineData(@"a", null, @"a"),
        InlineData(@"a", @"", @"a"),
        InlineData(@"a", @"b", @"a\b"),
        InlineData(@"a\", @"b", @"a\b"),
        InlineData(@"a/", @"b", @"a/b")
        ]
    public void CombineTests(string first, string second, string expected)
    {
        Paths.Combine(first, second).Should().Be(expected);

        StringBuilder sb = new(first);
        Paths.Combine(sb, second);
        sb.ToString().Should().Be(expected);
    }

    [Fact]
    public void NormalizeDirectorySeparatorsThrowsOnNull()
    {
        Action action = () => Paths.NormalizeDirectorySeparators(null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory,
        InlineData(@"", @""),
        InlineData(@"\", @"\"),
        InlineData(@"\\", @"\\"),
        InlineData(@"\\\", @"\"),
        InlineData(@"/", @"\"),
        InlineData(@"//", @"\\"),
        InlineData(@"///", @"\"),
        InlineData(@"/\/\", @"\"),
        InlineData(@"\/\/", @"\"),
        InlineData(@"C:\\", @"C:\"),
        InlineData(@"C:\a", @"C:\a"),
        InlineData(@"C:\\a", @"C:\a"),
        InlineData(@"C:\\a////////////b/", @"C:\a\b\")
        ]
    public void NormalizeDirectorySeparatorsTests(string input, string expected)
    {
        var result = Paths.NormalizeDirectorySeparators(input);
        result.Should().Be(expected);
        if (string.Equals(input, result, StringComparison.Ordinal))
        {
            // If they're equal we should get back the same instance
            result.Should().BeSameAs(input);
        }
    }

    [Theory,
        InlineData(@"", true, @"\\?\"),
        InlineData(@"", false, @""),
        InlineData(@"\\?\", true, @"\\?\"),
        InlineData(@"\\?\", false, @"\\?\"),
        InlineData(@"//?/", true, @"\\?\"),
        InlineData(@"//?/", false, @"//?/"),
        InlineData(@"\??\", true, @"\??\"),
        InlineData(@"\??\", false, @"\??\"),
        InlineData(@"/??/", true, @"\\?\"),
        InlineData(@"/??/", false, @"/??/"),
        InlineData(@"\\.\", true, @"\\?\"),
        InlineData(@"\\.\", false, @"\\.\"),
        InlineData(@"\\?\UNC\", true, @"\\?\UNC\"),
        InlineData(@"\\?\UNC\", false, @"\\?\UNC\"),
        InlineData(@"\\.\UNC\", true, @"\\?\UNC\"),
        InlineData(@"\\.\UNC\", false, @"\\.\UNC\"),
        InlineData(@"\\", true, @"\\?\UNC\"),
        InlineData(@"\\", false, @"\\")
        ]
    public void AddExtendedPrefixTests(string input, bool addIfUnderMaxPath, string expected)
    {
        Paths.AddExtendedPrefix(input, addIfUnderMaxPath).Should().Be(expected);
    }

    [Theory,
        InlineData(@"", @"", @""),
        InlineData(@"C:\Foo", @"C:\foo", @"C:\Foo"),
        InlineData(@"C:\Foo", @"C:\foo\", @"C:\Foo\"),
        InlineData(@"C:\Foo\", @"C:\foo", @"C:\Foo"),
        InlineData(@"C:\Foo\", @"C:\foo\", @"C:\Foo\"),
        InlineData(@"\\?\C:\Foo", @"C:\foo", @"C:\Foo"),
        InlineData(@"\\.\C:\Foo", @"C:\foo", @"C:\Foo"),
        InlineData(@"\\?\GLOBALROOT\GLOBAL??\C:\Foo", @"C:\foo", @"C:\Foo")
        ]
    public void ReplaceCasingTests(string sourcePath, string targetPath, string expected)
    {
        Paths.ReplaceCasing(sourcePath, targetPath).Should().Be(expected);
    }

    [Theory,
        InlineData(null, false),
        InlineData(@"", false),
        InlineData(@"\\?\", true),
        InlineData(@"\??\", true),
        InlineData(@"\\.\", true),
        InlineData(@"//?/", true),
        InlineData(@"/??/", true),
        InlineData(@"//./", true),
        InlineData(@"\\?", false),
        InlineData(@"\??", false),
        InlineData(@"\\.", false),
        InlineData(@"\\??", false),
        InlineData(@"\???", false),
        InlineData(@"\\..", false)
        ]
    public void IsDeviceTests(string path, bool expected)
    {
        Paths.IsDevice(path).Should().Be(expected);
    }

    [Theory,
        InlineData(null, false),
        InlineData(@"", false),
        InlineData(@"\\?\", true),
        InlineData(@"\??\", true),
        InlineData(@"\\.\", false),
        InlineData(@"//?/", false),
        InlineData(@"/??/", false),
        InlineData(@"\\?", false),
        InlineData(@"\??", false)
        ]
    public void IsExtendedTests(string path, bool expected)
    {
        Paths.IsExtended(path).Should().Be(expected);
    }

}
