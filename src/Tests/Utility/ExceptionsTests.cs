// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using XTask.Utility;
using Xunit;

namespace XTask.Tests.Utility
{
    public class ExceptionsTests
    {
        [Theory,
            MemberData(nameof(KnownExceptions))
            ]
        public void AreIoExceptions(Exception exception)
        {
            Exceptions.IsIoException(exception).Should().BeTrue();
        }

        public static IEnumerable<object[]> KnownExceptions
        {
            get
            {
                return new[]
                {
                    new object[] { new IOException() },
                    new object[] { new FileNotFoundException() },
                    new object[] { new ArgumentException() },
                    new object[] { new UnauthorizedAccessException() },
                    new object[] { new OperationCanceledException() }
                };
            }
        }
    }
}
