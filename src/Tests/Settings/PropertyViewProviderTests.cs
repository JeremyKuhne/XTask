// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Core.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using XTask.Settings;
    using Xunit;

    public class PropertyViewProviderTests
    {
        public sealed class TestViewAdapter : PropertyView
        {
            private TestViewAdapter(string value)
            {
                this.Value = value;
            }

            public string Value { get; private set; }

            public static IPropertyView Create(string value)
            {
                return new TestViewAdapter(value);
            }

            public override IEnumerator<IProperty<object>> GetEnumerator()
            {
                yield return new Property("Upper", this.Value.ToUpperInvariant());
                yield return new Property("Lower", this.Value.ToLowerInvariant());
            }

            public override string ToString()
            {
                return this.Value;
            }
        }

        private class TestObjectWithView : PropertyView
        {
            public override IEnumerator<IProperty<object>> GetEnumerator()
            {
                yield break;
            }
        }

        [Fact]
        public void IPropertyViewObject()
        {
            PropertyViewProvider provider = new PropertyViewProvider();
            TestObjectWithView testObject = new TestObjectWithView();
            provider.GetTypeView(testObject).ShouldBeEquivalentTo(testObject);
        }

        [Fact]
        public void RegisteredAdapter()
        {
            PropertyViewProvider provider = new PropertyViewProvider();
            provider.RegisterPropertyViewer<string>(TestViewAdapter.Create);
            string testObject = "Crazy!";
            IPropertyView view = provider.GetTypeView(testObject);
            view.ToString().Should().Be("Crazy!");
            view.Should().HaveCount(2);
            IProperty<object> property = view.First();
            property.Name.Should().Be("Upper");
            property.Value.Should().Be("CRAZY!");
        }

        [Fact]
        public void DefaultAdapter()
        {
            PropertyViewProvider provider = new PropertyViewProvider();
            Guid guid = Guid.NewGuid();
            IPropertyView view = provider.GetTypeView(guid);
            view.ToString().Should().Be(guid.ToString());
            view.Should().HaveCount(0);
        }
    }
}
