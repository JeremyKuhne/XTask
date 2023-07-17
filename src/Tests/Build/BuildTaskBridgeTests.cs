// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using XTask.Build;
using XTask.Settings;
using XTask.Tasks;
using Xunit;

namespace XTask.Tests.Build
{
    public class BuildTaskBridgeTests
    {
        private class TestBuildTaskBridge : BuildTaskBridge
        {
            public TestBuildTaskBridge()
                : base(null)
            {
            }

            public override ITaskService GetTaskService(ref IArgumentProvider argumentProvider)
            {
                throw new NotImplementedException();
            }
        }

        private class TestObjectWithView : PropertyView
        {
            public Dictionary<string, string> Properties = new();

            public override IEnumerator<IProperty<object>> GetEnumerator()
            {
                foreach (var pair in Properties)
                {
                    yield return new Property(pair.Key, pair.Value);
                }
            }
        }

        [Fact]
        public void TestOutput()
        {
            TestBuildTaskBridge bridge = new();
            TestObjectWithView testObject = new();
            testObject.Properties.Add("foo", "bar");
            testObject.Properties.Add("whiz", "bang");
            bridge.HandleOutput(testObject);

            var output = bridge.Output;
            output.Should().HaveCount(1, "only one item was output");
            output[0].MetadataCount.Should().Be(2, "two metadata items");
            output[0].GetMetadata("foo").Should().Be("bar");
            output[0].GetMetadata("whiz").Should().Be("bang");
            output[0].ItemSpec.Should().Be("XTask.Tests.Build.BuildTaskBridgeTests+TestObjectWithView", "should be default ToString()");
        }
    }
}
