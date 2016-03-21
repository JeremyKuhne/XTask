// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using XTask.Interop;
    using Xunit;
    using System.Linq;

    public class CryptographyTests
    {
        [Theory
            InlineData(StoreName.TrustedPeople)
            InlineData(StoreName.CertificateAuthority)
            ]
        public void OpenSystemStore(StoreName store)
        {
            NativeMethods.Cryptography.OpenSystemStore(store).IsInvalid.Should().BeFalse();
        }

        [Fact]
        public void BasicEnumerateStores()
        {
            string localMachineName = @"\\" + Environment.MachineName;
            var localMachine = NativeMethods.Cryptography.EnumerateSystemStores(NativeMethods.Cryptography.SystemStoreLocation.CERT_SYSTEM_STORE_LOCAL_MACHINE);
            localMachine.Should().NotBeEmpty();

            var localMachineByName = NativeMethods.Cryptography.EnumerateSystemStores(NativeMethods.Cryptography.SystemStoreLocation.CERT_SYSTEM_STORE_LOCAL_MACHINE, localMachineName);
            localMachineByName.Should().OnlyContain(x => x.Name.StartsWith(localMachineName, StringComparison.Ordinal), "when specifying the machine name they should come back with the name");
            localMachineByName.Should().Equal(localMachine, (s1, s2) => s1.Name.EndsWith(s2.Name, StringComparison.Ordinal), "names should be the same whether or not we get local by name");
        }

        [Fact]
        public void BasicEnumerateLocations()
        {
            string[] knownLocations = { "CurrentUser", "LocalMachine", "CurrentService", "Services", "Users", "CurrentUserGroupPolicy", "LocalMachineGroupPolicy", "LocalMachineEnterprise" };
            var locations = NativeMethods.Cryptography.EnumerateSystemStoreLocations();
            knownLocations.Should().BeSubsetOf(locations);
        }

        [Fact]
        public void BasicEnumeratePhysical()
        {
            string[] knownPhysical = { ".Default", ".AuthRoot", ".GroupPolicy", ".Enterprise" };
            var physical = NativeMethods.Cryptography.EnumeratePhysicalStores(NativeMethods.Cryptography.SystemStoreLocation.CERT_SYSTEM_STORE_LOCAL_MACHINE, "Root");
            knownPhysical.Should().BeSubsetOf(physical.Select(p => p.PhysicalStoreName));
        }
    }
}
