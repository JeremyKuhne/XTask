// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using XTask.Collections;
    using Xunit;

    public class DictionaryExtensionsTests
    {
        [Fact]
        public void UpdateIfPresent()
        {
            var dictionary = new Dictionary<string, string>();
            Action action = () => dictionary.UpdateIfPresent(null, "");
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");
            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");

            var dictionaryTwo = new Dictionary<int, int>();
            dictionaryTwo.Add(1, 1);
            dictionaryTwo.UpdateIfPresent(1, 2).Should().BeTrue("value was in dictionary");
            dictionaryTwo[1].Should().Be(2);
            dictionaryTwo.UpdateIfPresent(2, 3).Should().BeFalse();
        }

        [Fact]
        public void AddOrUpdate()
        {
            var dictionary = new Dictionary<string, string>();
            Action action = () => dictionary.AddOrUpdate(null, "");
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");
            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");

            var dictionaryTwo = new Dictionary<int, int>();
            dictionaryTwo.Add(1, 1);
            dictionaryTwo.AddOrUpdate(1, 2);
            dictionaryTwo[1].Should().Be(2);
            dictionaryTwo.AddOrUpdate(2, 3);
            dictionaryTwo[2].Should().Be(3);
        }

        [Fact]
        public void AddOrUpdateOverloadOne()
        {
            var dictionary = new Dictionary<string, string>();
            Func<string, string, string> updater = (key, oldValue) =>
            {
                key.Should().Be("fookey");
                oldValue.Should().Be("foo");
                return "bar";
            };

            Func<string, string, string> nullUpdater = null;

            dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: updater).Should().Be("foo");
            dictionary["fookey"].Should().Be("foo");
            dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: updater).Should().Be("bar");
            dictionary["fookey"].Should().Be("bar");

            // Null argument checks
            Action action = action = () => dictionary.AddOrUpdate(key: null, addValue: null, updateValueFactory: nullUpdater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            action = () => dictionary.AddOrUpdate(key: "foo", addValue: null, updateValueFactory: nullUpdater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("updateValueFactory");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void AddOrUpdateOverloadTwo()
        {
            var dictionary = new Dictionary<string, string>();
            Func<string, string> updater = (oldValue) =>
            {
                oldValue.Should().Be("foo");
                return "bar";
            };

            Func<string, string> nullUpdater = null;

            dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: updater).Should().Be("foo");
            dictionary["fookey"].Should().Be("foo");
            dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: updater).Should().Be("bar");
            dictionary["fookey"].Should().Be("bar");

            // Null argument checks
            Action action = action = () => dictionary.AddOrUpdate(key: null, addValue: null, updateValueFactory: nullUpdater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            action = () => dictionary.AddOrUpdate(key: "foo", addValue: null, updateValueFactory: nullUpdater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("updateValueFactory");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void AddOrUpdateOverloadThree()
        {
            var dictionary = new Dictionary<string, string>();
            Func<string, string, string> updater = (key, oldValue) =>
            {
                key.Should().Be("fookey");
                oldValue.Should().Be("foo");
                return "bar";
            };

            Func<string, string> adder = (key) =>
            {
                key.Should().Be("fookey");
                return "foo";
            };

            Func<string, string, string> nullUpdater = null;
            Func<string, string> nullAdder = null;

            dictionary.AddOrUpdate(key: "fookey", addValueFactory: adder, updateValueFactory: updater).Should().Be("foo");
            dictionary["fookey"].Should().Be("foo");
            dictionary.AddOrUpdate(key: "fookey", addValueFactory: adder, updateValueFactory: updater).Should().Be("bar");
            dictionary["fookey"].Should().Be("bar");

            // Null argument checks
            Action action = action = () => dictionary.AddOrUpdate(key: null, addValueFactory: adder, updateValueFactory: nullUpdater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            action = () => dictionary.AddOrUpdate(key: "foo", addValueFactory: adder, updateValueFactory: nullUpdater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("updateValueFactory");

            action = () => dictionary.AddOrUpdate(key: "foo", addValueFactory: nullAdder, updateValueFactory: updater);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("addValueFactory");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void GetOrAdd()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.GetOrAdd("fookey", "bar").Should().Be("bar");
            dictionary.GetOrAdd("fookey", "foo").Should().Be("bar");

            Action action = () => dictionary.GetOrAdd(null, "");
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void GetOrAddOverload()
        {
            var dictionary = new Dictionary<string, string>();

            Func<string, string> adder = (key) =>
            {
                key.Should().Be("fookey");
                return "bar";
            };

            dictionary.GetOrAdd("fookey", adder).Should().Be("bar");
            dictionary.GetOrAdd("fookey", adder).Should().Be("bar");

            Action action = () => dictionary.GetOrAdd(null, addValueFactory: adder);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            action = () => dictionary.GetOrAdd("fookey", addValueFactory: null);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("addValueFactory");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void TryRemoveValues()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "one", "foo" },
                { "two", "foo" },
                { "three", "bar" }
            };

            dictionary.TryRemoveValues("zing").Should().BeFalse();
            dictionary.TryRemoveValues("foo").Should().BeTrue();
            dictionary.Count.Should().Be(1);

            dictionary = null;
            Action action = () => dictionary.TryRemoveValues("zing");
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void TryRemove()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "three", "bar" }
            };

            dictionary.TryRemove("four").Should().BeFalse();
            dictionary.TryRemove("three").Should().BeTrue();
            dictionary.Count.Should().Be(0);

            Action action = () => dictionary.TryRemove(key: null);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void TryRemoveOverloadOne()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "one", "foo" },
                { "two", "foo" },
                { "three", "bar" }
            };

            dictionary.TryRemove(keys: new string[] { "zing" }).Should().BeFalse();
            dictionary.TryRemove(new string[] { "one", "three" }).Should().BeTrue();
            dictionary.Count.Should().Be(1);

            Action action = () => dictionary.TryRemove(keys: null);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("keys");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }

        [Fact]
        public void TryRemoveOverloadTwo()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "three", "bar" }
            };

            string removed;
            dictionary.TryRemove("four", out removed).Should().BeFalse();
            removed.Should().BeNull();
            dictionary.TryRemove("three", out removed).Should().BeTrue();
            removed.Should().Be("bar");
            dictionary.Count.Should().Be(0);

            Action action = () => dictionary.TryRemove(key: null, value: out removed);
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("key");

            dictionary = null;
            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("source");
        }
    }
}
