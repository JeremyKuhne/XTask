// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using FluentAssertions;
    using XTask.Utility;
    using Xunit;

    public class RegistryTests
    {
        private class TestRegistry : Registry
        {
            public static Exception TestRegistryExceptionWrapper(Action action)
            {
                return Registry.RegistryExceptionWrapper(action);
            }
        }

        [Theory,
            MemberData("KnownExceptions")]
        public void CatchesExpectedExceptions(Exception exception)
        {
            Action action = () => { throw exception; };
            TestRegistry.TestRegistryExceptionWrapper(action).Should().Be(exception);
        }

        public static IEnumerable<object[]> KnownExceptions
        {
            get
            {
                return new []
                {
                    new object[] { new ArgumentException() },
                    new object[] { new IOException() },
                    new object[] { new SecurityException() },
                    new object[] { new ObjectDisposedException("Foo") },
                    new object[] { new UnauthorizedAccessException() }
                };
            }
        }

        [Fact]
        public void DoesNotCatchUnknownExceptions()
        {
            Action action = () => { throw new Exception(); };
            action.Should().Throw<Exception>();
        }

        [Fact]
        public void BasicTests()
        {
            using (RegistryTestContext context = new RegistryTestContext())
            {
                Registry.RetrieveRegistryValue<string>(RegistryHive.CurrentUser, context.InstanceKey, "").Should().Be("DefaultValue", "default value for key returned with empty string");
                Registry.SetRegistryValue<string>(RegistryHive.CurrentUser, context.InstanceKey, null, "NewValue").Should().Be(true, "setting default value should be successful");
                Registry.RetrieveRegistryValue<string>(RegistryHive.CurrentUser, context.InstanceKey, null).Should().Be("NewValue", "default value for key returned with null");
                Registry.RetrieveRegistryValue<byte[]>(RegistryHive.CurrentUser, context.InstanceKey, "MeaningBinary").Should().Equal(RegistryTestContext.ByteArray, "byte array should be identical");
                Registry.RetrieveRegistryValue<string[]>(RegistryHive.CurrentUser, context.InstanceKey, "MeaningStringArray").Should().Equal(RegistryTestContext.StringArray, "string array should be identical");

                bool? triState = true;
                Registry.SetRegistryValue(RegistryHive.CurrentUser, context.InstanceKey, "Foo", triState).Should().BeTrue("successfully set triState bool with default type");
                Registry.RetrieveRegistryValue<bool>(RegistryHive.CurrentUser, context.InstanceKey, "Foo").Should().BeTrue("successfully retrieved bool? as bool");
                Registry.RetrieveRegistryValue<bool?>(RegistryHive.CurrentUser, context.InstanceKey, "Foo").Should().BeTrue("successfully retrieved bool? as bool?");
                triState = null;
                Registry.SetRegistryValue(RegistryHive.CurrentUser, context.InstanceKey, "Foo", triState).Should().BeTrue("successfully set tristate bool as null");
                Registry.RetrieveRegistryValue<bool?>(RegistryHive.CurrentUser, context.InstanceKey, "Foo").Should().NotHaveValue("successfully retrieved bool? as null bool?");

                Registry.GetSubkeyNames(RegistryHive.CurrentUser, context.InstanceKey).Should().HaveCount(2);
            }
        }

        [Fact]
        public void GetAllSubkeysTest_SetValueNoKey()
        {
            using (RegistryTestContext context = new RegistryTestContext())
            {
                string subkey = context.InstanceKey + @"\Flozzle";
                Registry.SetRegistryValue<string>(RegistryHive.CurrentUser, subkey, "Nozzle", "Wozzle").Should().BeTrue();
                Registry.RetrieveRegistryValue<string>(RegistryHive.CurrentUser, subkey, "Nozzle").Should().Be("Wozzle");
            }
        }

        [Fact]
        public void GetAllSubkeysTest_SubKeysNoValues()
        {
            using (RegistryTestContext context = new RegistryTestContext())
            {
                Registry.RetrieveAllRegistrySubkeyValues<string>(RegistryHive.CurrentUser, context.InstanceKey).Should().BeEmpty();
            }
        }

        [Fact]
        public void GetAllSubkeysTest_SubKeysValues()
        {
            using (RegistryTestContext context = new RegistryTestContext())
            {
                Registry.SetRegistryValue<string>(RegistryHive.CurrentUser, context.InstanceKey + @"\Foo", "Foo1", "one");
                Registry.SetRegistryValue<string>(RegistryHive.CurrentUser, context.InstanceKey + @"\Foo", "Foo2", "two");
                Registry.SetRegistryValue<string>(RegistryHive.CurrentUser, context.InstanceKey + @"\Bar", null, "three");
                Registry.RetrieveAllRegistrySubkeyValues<string>(RegistryHive.CurrentUser, context.InstanceKey).Should().BeEquivalentTo("one", "two", "three");
            }
        }

        private class RegistryTestContext : IDisposable
        {
            public string InstanceKey;
            private const string TestSubKey = @"Test\UnitTests\RegistryHelper\";
            public static byte[] ByteArray = new byte[] { 0x42, 0x42, 0x42, 0x70 };
            public static string[] StringArray = new string[] { "The", "meaning", "of", "life" };

            public RegistryTestContext()
            {
                this.InstanceKey = RegistryTestContext.TestSubKey + Guid.NewGuid().ToString();
                Microsoft.Win32.RegistryKey testSubKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(this.InstanceKey);

                testSubKey.SetValue(null, "DefaultValue");
                testSubKey.SetValue("MeaningDWord", 42, Microsoft.Win32.RegistryValueKind.DWord);
                testSubKey.SetValue("MeaningQWord", 42, Microsoft.Win32.RegistryValueKind.QWord);
                testSubKey.SetValue("MeaningString", "The meaning of life", Microsoft.Win32.RegistryValueKind.String);
                testSubKey.SetValue("MeaningStringArray", RegistryTestContext.StringArray, Microsoft.Win32.RegistryValueKind.MultiString);
                testSubKey.SetValue("MeaningBinary", RegistryTestContext.ByteArray, Microsoft.Win32.RegistryValueKind.Binary);
                testSubKey.CreateSubKey("Foo");
                testSubKey.CreateSubKey("Bar");
                testSubKey.Close();
            }

            public void Dispose()
            {
                Microsoft.Win32.RegistryKey testSubKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(this.InstanceKey);
                if (testSubKey != null)
                {
                    testSubKey.Close();
                    Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(this.InstanceKey);
                }
            }
        }
    }
}
