// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using FluentAssertions;
    using System;
    using System.Text;
    using XTask.Systems.File;
    using Xunit;

    public class PathsTests
    {
        static string[] TestPaths =
        {
            @"C:", @"C:Foo", @"C:\", @"C:\Foo", @"C", @"@:", @"[:", @"Foo", @"\", @"\Foo", @"/", @"/\", @"\\Foo\Bar", @"\\Foo\Bar\", @"\\?\\Foo\Bar",
            @"\\?\Foo\Bar", @"\\?\Foo\Bar\", @"\\?\UNC\\Foo\Bar", @"\\?\UNC\\", @"\\?\UNC\a\", @"\\?\UNC\a\\", @"\\?\UNC\a\b", @"\\?\UNC\Foo\Bar",
            @"\\?\UNC\Foo\Bar\", @"", @":", @"\\?\C:", @"\\?\@:\", @"\\?\[:\", @"\\?\C:\", @"\\?\C:\Foo", @"\\.psf\Home\", @"\\.", @"\\a\b", @"\\\",
            @"\\a\", @"\\a\\", @"\\.\"
        };

        public void PathFormatPerf()
        {
            string[] testPaths = PathsTests.TestPaths;
            const int times = 10000000;
            int root;
            DateTime start = DateTime.Now;
            for (int i = 0; i < times; i++)
            {
                for (int j = 0; j < testPaths.Length; j++)
                {
                    Paths.GetPathFormat(testPaths[j], out root);
                }
            }
            DateTime end = DateTime.Now;
            var elapsed = end - start;
        }

        [Theory,
            InlineData("", true),
            InlineData("C:", true),
            InlineData("**", true),
            InlineData(@"\\.\path", false),
            InlineData(@"\\?\path", false),
            InlineData(@"\\.", false),
            InlineData(@"\\?", false),
            InlineData(@"\\", false),
            InlineData(@"//", false),
            InlineData(@"\", true),
            InlineData(@"/", true),
            InlineData(@"C:Path", true),
            InlineData(@"C:\Path", false),
            InlineData(@"\\?\C:\Path", false),
            InlineData(@"Path", true),
            InlineData(@"X", true)]
        public void IsPathRelative(string path, bool expected)
        {
            Paths.IsRelative(path).Should().Be(expected);
        }

        [Theory,
            InlineData(null, PathFormat.UnknownFormat),
            InlineData(@" ", PathFormat.CurrentDirectoryRelative),
            InlineData(@"\\?\UNC\a\ ", PathFormat.UniformNamingConventionExtended),
            InlineData(@"\\a\ ", PathFormat.UniformNamingConvention),
            InlineData(@"\\.\ ", PathFormat.Device)
            InlineData(@"", PathFormat.UnknownFormat),
            InlineData(@"\\?\UNC\a\", PathFormat.UnknownFormat),
            InlineData(@"\\a\", PathFormat.UnknownFormat),
            InlineData(@"\\.\", PathFormat.UnknownFormat)]
        public void PathFormatChangesWithTrailingSpace(string path, PathFormat expected)
        {
            Paths.GetPathFormat(path).Should().Be(expected, "Passed path was '{0}'", path);
        }

        [Theory,
            InlineData(@"C:", PathFormat.DriveRelative),
            InlineData(@"C:Foo", PathFormat.DriveRelative),
            InlineData(@"C:\", PathFormat.DriveAbsolute),
            InlineData(@"C:\Foo", PathFormat.DriveAbsolute),
            InlineData(@"C", PathFormat.CurrentDirectoryRelative),
            InlineData(@"@:", PathFormat.UnknownFormat),
            InlineData(@"[:", PathFormat.UnknownFormat),
            InlineData(@"Foo", PathFormat.CurrentDirectoryRelative),
            InlineData(@"\", PathFormat.CurrentVolumeRelative),
            InlineData(@"\Foo", PathFormat.CurrentVolumeRelative),
            InlineData(@"/", PathFormat.CurrentVolumeRelative),
            InlineData(@"/\", PathFormat.UnknownFormat),
            InlineData(@"\\Foo\Bar", PathFormat.UniformNamingConvention),
            InlineData(@"\\Foo\Bar\", PathFormat.UniformNamingConvention),
            InlineData(@"\\?\\Foo\Bar", PathFormat.UnknownFormat),
            InlineData(@"\\?\Foo\Bar", PathFormat.VolumeAbsoluteExtended),
            InlineData(@"\\?\Foo\Bar\", PathFormat.VolumeAbsoluteExtended),
            InlineData(@"\\?\UNC\\Foo\Bar", PathFormat.UnknownFormat),
            InlineData(@"\\?\UNC\\", PathFormat.UnknownFormat),
            InlineData(@"\\?\UNC\a\\", PathFormat.UnknownFormat),
            InlineData(@"\\?\UNC\a\b", PathFormat.UniformNamingConventionExtended),
            InlineData(@"\\?\UNC\Foo\Bar", PathFormat.UniformNamingConventionExtended),
            InlineData(@"\\?\UNC\Foo\Bar\", PathFormat.UniformNamingConventionExtended),
            InlineData(@":", PathFormat.UnknownFormat),
            InlineData(@"\\?\C:", PathFormat.VolumeAbsoluteExtended),
            InlineData(@"\\?\@:\", PathFormat.VolumeAbsoluteExtended),
            InlineData(@"\\?\[:\", PathFormat.VolumeAbsoluteExtended),
            InlineData(@"\\?\C:\", PathFormat.VolumeAbsoluteExtended),
            InlineData(@"\\?\C:\Foo", PathFormat.VolumeAbsoluteExtended),
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
            InlineData(@"\\?\\Foo\Bar", null),
            InlineData(@"\\?\Foo\Bar", @"\\?\Foo\"),
            InlineData(@"\\?\Foo\Bar\", @"\\?\Foo\"),
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
            InlineData(@"\\.psf\Home\", @"\\.psf\Home\"),
            InlineData(@"\\.", null),
            InlineData(@"\\a\b", @"\\a\b"),
            InlineData(@"\\\", null),
            InlineData(@"\\a\", null),
            InlineData(@"\\a\\", null),
            InlineData(@"\\.\", null)]
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
            InlineData(null, new string[0])
            InlineData(new[] { "" }, new string[0])
            InlineData(new[] { @"C:\Foo\Bar\", @"C:\Foo\" }, new[] { @"C:\Foo\" })
            InlineData(new[] { @"C:\", @"D:\Foo\Bar.txt" }, new[] { @"C:\", @"D:\Foo\" })
            InlineData(new[] { @"C:\Bar\Bar.txt", @"C:\Foo\Car", @"C:\Foo\Bar.txt", @"\\LocalHost\Share", @"\\LocalHost\Share\", @"C:\Bar\", @"\\LocalHost\Share\Foo\Bar.txt", @"C:\Foo\Bar\Foo.txt", },
                        new[] { @"C:\Bar\", @"C:\Foo\", @"\\LocalHost\Share\" })
            InlineData(new[] { @"C:\A\B\C.txt", @"C:\A\B\C\D.txt", @"C:\B\A", @"C:\B\A\", @"C:\C\A\B\A.txt", @"C:\C\A\B\C\C.txt", @"C:\A\B\C.txt", @"C:\D\A.txt" },
                        new[] { @"C:\A\B\", @"C:\B\", @"C:\C\A\B\", @"C:\D\" })
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
            action.ShouldThrow<ArgumentNullException>("null string should throw");

            action = () => Paths.Combine((StringBuilder)null, null);
            action.ShouldThrow<ArgumentNullException>("null stringbuilder should throw");
        }

        [Theory
            InlineData(@"a", null, @"a")
            InlineData(@"a", @"", @"a")
            InlineData(@"a", @"b", @"a\b")
            InlineData(@"a\", @"b", @"a\b")
            InlineData(@"a/", @"b", @"a/b")
            ]
        public void CombineTests(string first, string second, string expected)
        {
            Paths.Combine(first, second).Should().Be(expected);

            StringBuilder sb = new StringBuilder(first);
            Paths.Combine(sb, second);
            sb.ToString().Should().Be(expected);
        }

    }
}
