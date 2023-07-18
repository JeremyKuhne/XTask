// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using XTask.Collections;
using Xunit;

namespace XTask.Tests.Collections;

public class DictionaryExtensionsTests
{
    [Fact]
    public void UpdateIfPresent()
    {
        Dictionary<string, string> dictionary = new();
        Action action = () => dictionary.UpdateIfPresent(null, "");
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");
        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");

        Dictionary<int, int> dictionaryTwo = new()
        {
            { 1, 1 }
        };

        dictionaryTwo.UpdateIfPresent(1, 2).Should().BeTrue("value was in dictionary");
        dictionaryTwo[1].Should().Be(2);
        dictionaryTwo.UpdateIfPresent(2, 3).Should().BeFalse();
    }

    [Fact]
    public void AddOrUpdate()
    {
        Dictionary<string, string> dictionary = new();
        Action action = () => dictionary.AddOrUpdate(null, "");
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");
        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");

        Dictionary<int, int> dictionaryTwo = new()
        {
            { 1, 1 }
        };

        dictionaryTwo.AddOrUpdate(1, 2);
        dictionaryTwo[1].Should().Be(2);
        dictionaryTwo.AddOrUpdate(2, 3);
        dictionaryTwo[2].Should().Be(3);
    }

    [Fact]
    public void AddOrUpdateOverloadOne()
    {
        Dictionary<string, string> dictionary = new();

        static string Updater(string key, string oldValue)
        {
            key.Should().Be("fookey");
            oldValue.Should().Be("foo");
            return "bar";
        }

        Func<string, string, string> nullUpdater = null;

        dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: Updater).Should().Be("foo");
        dictionary["fookey"].Should().Be("foo");
        dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: Updater).Should().Be("bar");
        dictionary["fookey"].Should().Be("bar");

        // Null argument checks
        Action action = action = () => dictionary.AddOrUpdate(key: null, addValue: null, updateValueFactory: nullUpdater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        action = () => dictionary.AddOrUpdate(key: "foo", addValue: null, updateValueFactory: nullUpdater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("updateValueFactory");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void AddOrUpdateOverloadTwo()
    {
        Dictionary<string, string> dictionary = new();

        static string Updater(string oldValue)
        {
            oldValue.Should().Be("foo");
            return "bar";
        }

        Func<string, string> nullUpdater = null;

        dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: Updater).Should().Be("foo");
        dictionary["fookey"].Should().Be("foo");
        dictionary.AddOrUpdate(key: "fookey", addValue: "foo", updateValueFactory: Updater).Should().Be("bar");
        dictionary["fookey"].Should().Be("bar");

        // Null argument checks
        Action action = action = () => dictionary.AddOrUpdate(key: null, addValue: null, updateValueFactory: nullUpdater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        action = () => dictionary.AddOrUpdate(key: "foo", addValue: null, updateValueFactory: nullUpdater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("updateValueFactory");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void AddOrUpdateOverloadThree()
    {
        Dictionary<string, string> dictionary = new();

        static string Updater(string key, string oldValue)
        {
            key.Should().Be("fookey");
            oldValue.Should().Be("foo");
            return "bar";
        }

        static string Adder(string key)
        {
            key.Should().Be("fookey");
            return "foo";
        }

        Func<string, string, string> nullUpdater = null;
        Func<string, string> nullAdder = null;

        dictionary.AddOrUpdate(key: "fookey", addValueFactory: Adder, updateValueFactory: Updater).Should().Be("foo");
        dictionary["fookey"].Should().Be("foo");
        dictionary.AddOrUpdate(key: "fookey", addValueFactory: Adder, updateValueFactory: Updater).Should().Be("bar");
        dictionary["fookey"].Should().Be("bar");

        // Null argument checks
        Action action = action = () => dictionary.AddOrUpdate(key: null, addValueFactory: Adder, updateValueFactory: nullUpdater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        action = () => dictionary.AddOrUpdate(key: "foo", addValueFactory: Adder, updateValueFactory: nullUpdater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("updateValueFactory");

        action = () => dictionary.AddOrUpdate(key: "foo", addValueFactory: nullAdder, updateValueFactory: Updater);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("addValueFactory");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void GetOrAdd()
    {
        Dictionary<string, string> dictionary = new();
        dictionary.GetOrAdd("fookey", "bar").Should().Be("bar");
        dictionary.GetOrAdd("fookey", "foo").Should().Be("bar");

        Action action = () => dictionary.GetOrAdd(null, "");
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void GetOrAddOverload()
    {
        Dictionary<string, string> dictionary = new();

        static string Adder(string key)
        {
            key.Should().Be("fookey");
            return "bar";
        }

        dictionary.GetOrAdd("fookey", Adder).Should().Be("bar");
        dictionary.GetOrAdd("fookey", Adder).Should().Be("bar");

        Action action = () => dictionary.GetOrAdd(null, addValueFactory: Adder);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        action = () => dictionary.GetOrAdd("fookey", addValueFactory: null);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("addValueFactory");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void TryRemoveValues()
    {
        Dictionary<string, string> dictionary = new()
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
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void TryRemove()
    {
        Dictionary<string, string> dictionary = new()
        {
            { "three", "bar" }
        };

        dictionary.TryRemove("four").Should().BeFalse();
        dictionary.TryRemove("three").Should().BeTrue();
        dictionary.Count.Should().Be(0);

        Action action = () => dictionary.TryRemove(key: null);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void TryRemoveOverloadOne()
    {
        Dictionary<string, string> dictionary = new()
        {
            { "one", "foo" },
            { "two", "foo" },
            { "three", "bar" }
        };

        dictionary.TryRemove(keys: new string[] { "zing" }).Should().BeFalse();
        dictionary.TryRemove(new string[] { "one", "three" }).Should().BeTrue();
        dictionary.Count.Should().Be(1);

        Action action = () => dictionary.TryRemove(keys: null);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("keys");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }

    [Fact]
    public void TryRemoveOverloadTwo()
    {
        Dictionary<string, string> dictionary = new()
        {
            { "three", "bar" }
        };

        dictionary.TryRemove("four", out string removed).Should().BeFalse();
        removed.Should().BeNull();
        dictionary.TryRemove("three", out removed).Should().BeTrue();
        removed.Should().Be("bar");
        dictionary.Count.Should().Be(0);

        Action action = () => dictionary.TryRemove(key: null, value: out removed);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("key");

        dictionary = null;
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("source");
    }
}
