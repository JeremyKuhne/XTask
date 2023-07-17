// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using XTask.Systems.File;
using Xunit;
using XTask.Tests.Support;

namespace XTask.Tests.Utility
{
    public class FilesTests
    {
        [Fact]
        public void BasicReadFileLinesTest()
        {
            IFileService fileService = TestFileServices.CreateSubstituteForFile(out string path, "a\nb\nc");
            string[] lines = fileService.ReadLines(path).ToArray();
            lines.Should().ContainInOrder("a", "b", "c");
        }

        [Theory,
            InlineData("", 1),
            InlineData("\r", 1),
            InlineData("\r\n", 1),
            InlineData("\r ", 1),
            InlineData("\n\n\n", 3),
            InlineData("\r\n \r\n \n ", 4)]
        public void CountLinesTest(string content, int expected)
        {
            IFileService fileService = TestFileServices.CreateSubstituteForFile(out string path, content);
            fileService.CountLines(path).Should().Be(expected);
        }

        [Theory,
            InlineData("foo", "foo", true, true),
            InlineData("FoO", "foo", true, true),
            InlineData("Foo", "foo", false, false),
            InlineData("Foo", @"[aeiou]{2,}", false, true),   // Two consecutive vowels
            InlineData("FoO", @"[aeiou]{2,}", false, false),  // Two consecutive vowels
            InlineData("FoO", @"[aeiou]{2,}", true, true),    // Two consecutive vowels, case insensitive
            InlineData("foo", "bar", true, false),
            InlineData("foo", "bar", false, false)]
        public void ContainsRegexTest(string content, string regex, bool ignoreCase, bool expected)
        {
            IFileService fileService = TestFileServices.CreateSubstituteForFile(out string path, content);
            IEnumerable<string> containingPaths = fileService.ContainsRegex(regex: regex, ignoreCase: ignoreCase, paths: new string[] { path });
            if (expected)
            {
                containingPaths.Should().Contain(path);
            }
            else
            {
                containingPaths.Should().NotContain(path);
            }
        }

        [Theory,
            InlineData("[aeiou]{2,}", "[aeiou]{2,}", true, true),
            InlineData("FoO", "[aeiou]{2,}", true, false)]
        public void ContainsStringTest(string content, string value, bool ignoreCase, bool expected)
        {
            IFileService fileService = TestFileServices.CreateSubstituteForFile(out string path, content);
            IEnumerable<string> containingPaths = fileService.ContainsString(value: value, ignoreCase: ignoreCase, paths: new string[] { path });
            if (expected)
            {
                containingPaths.Should().Contain(path);
            }
            else
            {
                containingPaths.Should().NotContain(path);
            }
        }
    }
}
