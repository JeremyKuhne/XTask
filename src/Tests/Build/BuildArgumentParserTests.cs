// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using XTask.Build;
using Xunit;

namespace XTask.Tests.Build
{
    public class BuildArgumentParserTests
    {
        [Fact]
        public void BasicTaskTest()
        {
            BuildArgumentParser parser = new("task", null, null, null);
            parser.Command.Should().Be("task");
        }

        [Fact]
        public void BasicTargetsTest()
        {
            BuildArgumentParser parser = new("task", new string[] { "foo", "bar" }, null, null);
            parser.Target.Should().Be("foo");
            parser.Targets.Should().ContainInOrder("foo", "bar");
        }

        [Fact]
        public void BasicOptionTest()
        {
            string options = @"<Foo>Bar</Foo>";
            BuildArgumentParser parser = new("task", null, options, null);
            parser.GetOption<string>("foo").Should().Be("Bar");
        }

        [Fact]
        public void MultiOptionTest()
        {
            string options = @"<Foo>Bar</Foo><Foo>Foo</Foo>";
            BuildArgumentParser parser = new("task", null, options, null);
            parser.GetOption<string>("foo").Should().Be("Bar;Foo");
        }
    }
}
