// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Interop;
    using XTask.Systems.File;
    using Support;
    using Xunit;
    using System.Security.Cryptography.X509Certificates;

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
    }
}
