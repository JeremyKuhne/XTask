// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FluentAssertions;
    using System;
    using XTask.Interop;
    using Xunit;
    using System.Linq;

    public class NetworkManagementTests
    {
        [Fact]
        public void BasicGetLocalGroupNames()
        {
            string[] knownLocalGroups = { "Administrators", "Guests", "Users" };
            var localGroups = NativeMethods.NetworkManagement.EnumerateLocalGroups();
            localGroups.Should().Contain(knownLocalGroups);
            knownLocalGroups.Should().BeSubsetOf(localGroups);
        }

        [Fact]
        public void BasicGetLocalGroupMembers()
        {
            string[] knownMembers = { "Authenticated Users", "INTERACTIVE" };
            var members = NativeMethods.NetworkManagement.EnumerateGroupUsers("Users");
            members.Select(m => m.Name).Should().Contain(knownMembers);
            knownMembers.Should().BeSubsetOf(members.Select(m => m.Name));
        }
    }
}
