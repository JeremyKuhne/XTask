// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using XTask.Utility;
using Xunit;

namespace XTask.Tests.Utility
{
    public class TypesTests
    {
        [Fact]
        public void NullValue()
        {
            Types.ConvertType<int>(null).Should().Be(0);
            Types.ConvertType<int?>(null).Should().NotHaveValue();
            Types.ConvertType<string>(null).Should().BeNull();
        }

        [Fact]
        public void StringToIntrinsic()
        {
            string three = "3";
            Types.ConvertType<int>(three).Should().Be(3, "int");
            Types.ConvertType<uint>(three).Should().Be(3, "uint");
            Types.ConvertType<short>(three).Should().Be(3, "short");
            Types.ConvertType<ushort>(three).Should().Be(3, "ushort");
            Types.ConvertType<long>(three).Should().Be(3, "long");
            Types.ConvertType<ulong>(three).Should().Be(3, "ulong");
            Types.ConvertType<float>(three).Should().Be(3.0f, "float");
            Types.ConvertType<double>(three).Should().Be(3.0d, "double");
            Types.ConvertType<byte>(three).Should().Be(3, "byte");
            Types.ConvertType<sbyte>(three).Should().Be(3, "sbyte");
            Types.ConvertType<char>(three).Should().Be('3', "char");
            Types.ConvertType<decimal>(three).Should().Be(3.0m, "decimal");
        }

        [Fact]
        public void StringToNullableIntrinsic()
        {
            string three = "3";
            Types.ConvertType<int?>(three).Value.Should().Be(3, "int");
            Types.ConvertType<uint?>(three).Value.Should().Be(3, "uint");
            Types.ConvertType<short?>(three).Value.Should().Be(3, "short");
            Types.ConvertType<ushort?>(three).Value.Should().Be(3, "ushort");
            Types.ConvertType<long?>(three).Value.Should().Be(3, "long");
            Types.ConvertType<ulong?>(three).Value.Should().Be(3, "ulong");
            Types.ConvertType<float?>(three).Value.Should().Be(3.0f, "float");
            Types.ConvertType<double?>(three).Value.Should().Be(3.0d, "double");
            Types.ConvertType<byte?>(three).Value.Should().Be(3, "byte");
            Types.ConvertType<sbyte?>(three).Value.Should().Be(3, "sbyte");
            Types.ConvertType<char?>(three).Value.Should().Be('3', "char");
            Types.ConvertType<decimal?>(three).Value.Should().Be(3.0m, "decimal");
        }

        private class Foo { }
        private class Bar : Foo { }

        [Fact]
        public void AssignableFrom()
        {
            Foo foo = new();
            Bar bar = new();

            Types.ConvertType<Foo>(foo).Should().Be(foo);
            Types.ConvertType<Bar>(bar).Should().Be(bar);
            Types.ConvertType<Foo>(bar).Should().Be(bar);
            Types.ConvertType<Bar>(foo).Should().BeNull();
        }

        private enum Numbers
        {
            One,
            Two,
            Three
        }

        private enum OtherNumbers
        {
            One = 1,
            Two = 2,
            Three = 3
        }

        private enum NoValues
        {
        }

        [Fact]
        public void Enums()
        {
            Types.ConvertType<Numbers>("One").Should().Be(Numbers.One);
            Types.ConvertType<Numbers>("Fizzle").Should().Be(Numbers.One);
            Types.ConvertType<Numbers?>("Fizzle").Should().BeNull();
            Types.ConvertType<Numbers>(4).Should().Be(Numbers.One);
            Types.ConvertType<Numbers?>(4).Should().BeNull("four isn't defined, asked for nullable");
            Types.ConvertType<Numbers>(1).Should().Be(Numbers.Two);
            Types.ConvertType<Numbers>(Numbers.Three).Should().Be(Numbers.Three);
            Types.ConvertType<OtherNumbers>(Numbers.Three).Should().Be(OtherNumbers.Three);
            Types.ConvertType<OtherNumbers>(4).Should().Be(OtherNumbers.One, "four isn't defined");
            Types.ConvertType<NoValues>(1).Should().Be(0, "nothing defined in enum, fallback to default");
        }

        private class FooBar
        {
            public override string ToString()
            {
                return "WingNut";
            }
        }

        [Fact]
        public void StringFallback()
        {
            Types.ConvertType<string>(new FooBar()).Should().Be("WingNut");
        }

        [Fact]
        public void Boolean()
        {
            Types.ConvertType<bool>(0).Should().BeFalse();
            Types.ConvertType<bool>(1).Should().BeTrue();
            Types.ConvertType<bool>("0").Should().BeFalse();
            Types.ConvertType<bool>("1").Should().BeTrue();
            Types.ConvertType<bool>((uint)0).Should().BeFalse();
            Types.ConvertType<bool>((uint)1).Should().BeTrue();
            Types.ConvertType<bool>(0.0f).Should().BeFalse();
            Types.ConvertType<bool>(1.0f).Should().BeTrue();
            Types.ConvertType<bool>(0.0d).Should().BeFalse();
            Types.ConvertType<bool>(1.0d).Should().BeTrue();
            Types.ConvertType<bool>("TRUE").Should().BeTrue();
            Types.ConvertType<bool>("true").Should().BeTrue();
            Types.ConvertType<bool>("FALSE").Should().BeFalse();
            Types.ConvertType<bool>("false").Should().BeFalse();
        }
    }
}
