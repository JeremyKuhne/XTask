// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XTask.Settings;

namespace XTask.Tests.Settings;

public class PropertyViewProviderTests
{
    public sealed class TestViewAdapter : PropertyView
    {
        private TestViewAdapter(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static IPropertyView Create(string value)
        {
            return new TestViewAdapter(value);
        }

        public override IEnumerator<IProperty<object>> GetEnumerator()
        {
            yield return new Property("Upper", Value.ToUpperInvariant());
            yield return new Property("Lower", Value.ToLowerInvariant());
        }

        public override string ToString()
        {
            return Value;
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
        PropertyViewProvider provider = new();
        TestObjectWithView testObject = new();
        provider.GetTypeView(testObject).Should().Equal(testObject);
    }

    [Fact]
    public void RegisteredAdapter()
    {
        PropertyViewProvider provider = new();
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
        PropertyViewProvider provider = new();
        Guid guid = Guid.NewGuid();
        IPropertyView view = provider.GetTypeView(guid);
        view.ToString().Should().Be(guid.ToString());
        view.Should().HaveCount(0);
    }
}
