// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Build
{
    using FluentAssertions;
    using XTask.Build;
    using Xunit;

    public class BuildArgumentParserTests
    {
        [Fact]
        public void BasicTaskTest()
        {
            BuildArgumentParser parser = new BuildArgumentParser("task", null, null);
            parser.Command.Should().Be("task");
        }

        [Fact]
        public void BasicTargetsTest()
        {
            BuildArgumentParser parser = new BuildArgumentParser("task", new string[] { "foo", "bar" }, null);
            parser.Target.Should().Be("foo");
            parser.Targets.Should().ContainInOrder("foo", "bar");
        }

        [Fact]
        public void BasicOptionTest()
        {
            string options = @"<Foo>Bar</Foo>";
            BuildArgumentParser parser = new BuildArgumentParser("task", null, options);
            parser.GetOption<string>("foo").Should().Be("Bar");
        }

        [Fact]
        public void MultiOptionTest()
        {
            string options = @"<Foo>Bar</Foo><Foo>Foo</Foo>";
            BuildArgumentParser parser = new BuildArgumentParser("task", null, options);
            parser.GetOption<string>("foo").Should().Be("Bar;Foo");
        }
    }
}
