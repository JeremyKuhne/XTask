// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using XTask.Systems.File;
using XTask.Settings;
using Xunit;
using XTask.Tests.Support;

namespace XTask.Tests.Utility
{
    public class ArgumentParserTests
    {
        [Fact]
        public void NullParse()
        {
            CommandLineParser parser = new(null);
            parser.Parse(null);

            parser.Target.Should().BeNull("null argument set should have null target");
            parser.Targets.Should().NotBeNull("null argument set should not have null targets");
            parser.Targets.Should().HaveCount(0, "null argument set should have no targets");
        }

        [Fact]
        public void EmptyParse()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[0]);
            parser.Target.Should().BeNull("empty argument set should have null target");
            parser.Targets.Should().NotBeNull("empty argument set should not have null targets");
            parser.Targets.Should().HaveCount(0, "empty argument set should have no targets");
        }

        [Fact]
        public void EffectivelyEmptyArgumentsParse()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { null, "" });
            parser.Target.Should().BeNull("effectively empty argument set should have null target");
            parser.Targets.Should().NotBeNull("effectively empty argument set should not have null targets");
            parser.Targets.Should().HaveCount(0, "effectively empty argument set should have no targets");
        }

        [Fact]
        public void BasicSwitchArguments()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { @"/ foo", @"-BAR" });
            parser.Target.Should().BeNull("only switch arguments should have null target");
            parser.Targets.Should().NotBeNull("only switch arguments should not have null targets");
            parser.Targets.Should().HaveCount(0, "only switch arguments should have no targets");
            parser.GetOption<bool>("foo").Should().BeTrue("foo switch should be true, even though it has a space");
            parser.GetOption<bool>("bar").Should().BeTrue("bar switch was set");
            parser.GetOption<bool>("Bar").Should().BeTrue("foo switch should be true, irrespective of case");
        }

        [Fact]
        public void StringArgument()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { @"/foo:bar" });
            parser.Target.Should().BeNull("only switch arguments should have null target");
            parser.Targets.Should().NotBeNull("only switch arguments should not have null targets");
            parser.Targets.Should().HaveCount(0, "only switch arguments should have no targets");
            parser.GetOption<string>("foo").Should().Be("bar", "foo's value was specified as bar");
            parser.GetOption<bool>("foo").Should().BeFalse("foo switch doesn't translate to bool");
            parser.GetOption<bool?>("foo").Should().NotHaveValue("foo switch doesn't translate to bool");
        }

        [Fact]
        public void MultipartArgument()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { @"/foo:bar", @"/foo:foo" });
            parser.Target.Should().BeNull("only switch arguments should have null target");
            parser.Targets.Should().NotBeNull("only switch arguments should not have null targets");
            parser.Targets.Should().HaveCount(0, "only switch arguments should have no targets");
            parser.GetOption<string>("foo").Should().Be("bar;foo", "foo's value was specified via two switches");
        }

        [Fact]
        public void FileArgument()
        {
            IFileService fileService = TestFileServices.CreateSubstituteForFile(out string path, "a\nb\nc\n@ @ @");
            CommandLineParser parser = new(fileService);

            parser.Parse(new string[] { "Command", @"/foo:@" + path });
            parser.GetOption<string>("foo").Should().Be("a;b;c", "specified in file as a, b, c");
        }

        [Fact]
        public void BooleanSwitches()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { @"/foo", @"/bar:1", @"/foofoo:0", @"/barbar:false", @"/foobar:true" });
            parser.Target.Should().BeNull("only switch arguments should have null target");
            parser.Targets.Should().NotBeNull("only switch arguments should not have null targets");
            parser.Targets.Should().HaveCount(0, "only switch arguments should have no targets");
            parser.GetOption<bool>("foo").Should().BeTrue("foo switch should be true");
            parser.GetOption<int>("foo").Should().Be(1, "foo switch should be also be 1");
            parser.GetOption<bool>("bar").Should().BeTrue("far switch should be true");
            parser.GetOption<int>("bar").Should().Be(1, "bar switch should be also be 1");
            parser.GetOption<bool>("foofoo").Should().BeFalse("foofoo switch should be false");
            parser.GetOption<int>("foofoo").Should().Be(0, "foofoo switch should be also be 0");
            parser.GetOption<bool>("barbar").Should().BeFalse("barbar switch should be false");
            parser.GetOption<int>("barbar").Should().Be(0, "barbar switch should be also be 0");
            parser.GetOption<bool>("foobar").Should().BeTrue("explicit foobar switch should be true");

            // TODO: Need to handle the float/double case
            // parser.GetOption<double>("foobar").Should().Be(1.0, "explicit foobar switch should be 1.0");
        }

        [Fact]
        public void SimpleCommand()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "command" });
            parser.Command.Should().Be("command", "command was specified");
            parser.Target.Should().BeNull("no targets specified");
            parser.Targets.Should().HaveCount(0, "no targets specified");
        }

        [Fact]
        public void SingleTarget()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "command", "target" });
            parser.Command.Should().Be("command", "command was specified first");
            parser.Target.Should().Be("target", "single target should be target");
            parser.HelpRequested.Should().BeFalse("did not request help");
            parser.Targets.Should().HaveCount(1, "only specified one target");
        }

        [Fact]
        public void MultipleTarget()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "command", "targetone", "targettwo" });
            parser.Command.Should().Be("command", "command was specified first");
            parser.Target.Should().Be("targetone", "first target should be target");
            parser.Targets.Should().HaveCount(2, "should have two targets");
        }

        [Fact]
        public void FileTarget()
        {
            IFileService fileService = TestFileServices.CreateSubstituteForFile(out string path, "a\nb\nc\n@ @ @");
            CommandLineParser parser = new(fileService);

            parser.Parse(new string[] { "Command", @"@" + path });
            parser.Targets.Should().ContainInOrder("a", "b", "c");
        }

        [Fact]
        public void NoCommand()
        {
            CommandLineParser parser = new(null);
            parser.Parse(null);
            parser.Command.Should().Be("help", "no command should come back as help");
            parser.Target.Should().BeNull("no targets specified");
            parser.Targets.Should().HaveCount(0, "no targets specified");
        }

        [Fact]
        public void QuestionHelp()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "?" });
            parser.Command.Should().Be("help", "no command should come back as help");
            parser.HelpRequested.Should().BeTrue("requested help");
            parser.Target.Should().BeNull("no targets specified");
            parser.Targets.Should().HaveCount(0, "no targets specified");
        }

        [Fact]
        public void UnspecifiedHelp()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "HeLp" });
            parser.Command.Should().Be("help", "no command should come back as help");
            parser.HelpRequested.Should().BeTrue("requested help");
            parser.Target.Should().BeNull("no targets specified");
            parser.Targets.Should().HaveCount(0, "no targets specified");
        }

        [Fact]
        public void TargetedHelp()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "Help", "command" });
            parser.Command.Should().Be("command", "command for help was specified as command");
            parser.HelpRequested.Should().BeTrue("requested help");
            parser.Target.Should().BeNull("no targets specified");
            parser.Targets.Should().HaveCount(0, "no targets specified");
        }

        [Fact]
        public void ReversedHelp()
        {
            CommandLineParser parser = new(null);
            parser.Parse(new string[] { "command", "help" });
            parser.Command.Should().Be("command", "command for help was specified as command");
            parser.HelpRequested.Should().BeTrue("requested help");
            parser.Target.Should().BeNull("no targets specified");
            parser.Targets.Should().HaveCount(0, "no targets specified");
        }
    }
}
