// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using NSubstitute;
using XTask.Systems.File;
using XTask.Settings;
using XTask.Utility;
using Xunit;
using System.IO;

namespace XTask.Tests.Utility
{
    public class AssemblyResolverTests
    {
        public class TestAssemblyResolver : AssemblyResolver
        {
            protected TestAssemblyResolver(IFileService fileService)
                : base(fileService)
            {
            }

            public static TestAssemblyResolver TestCreate(IArgumentProvider arguments, IFileService fileService)
            {
                TestAssemblyResolver resolver = new(fileService);
                resolver.Initialize(arguments);
                return resolver;
            }

            public void TestInitialize(IArgumentProvider arguments)
            {
                Initialize(arguments);
                AssemblyResolveFallback += TestFallBack_AssemblyResolve;
            }

            public void TestLoadAssemblyFrom(string fullAssembly)
            {
            }

            protected override Uri GetToolLocation()
            {
                return new Uri(@"C:\TestToolLocation");
            }

            protected override Assembly LoadAssemblyFrom(string fullAssemblyPath)
            {
                TestLoadAssemblyFrom(fullAssemblyPath);
                return Assembly.GetExecutingAssembly();
            }

            public IEnumerable<string> ResolutionPaths
            {
                get { return _resolutionPaths; }
            }

            public IEnumerable<string> AssembliesToResolve
            {
                get { return _assembliesToResolve; }
            }

            public Assembly TestFallBack_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                return null;
            }
        }

        [Fact]
        public void ParameterPassingTest()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>(StandardOptions.AssembliesToResolve).Returns("foo;bar");
            arguments.GetOption<string>(StandardOptions.AssemblyResolutionPaths).Returns(@"C:\TestLoad1;C:\TestLoad2");
            TestAssemblyResolver testResolver = TestAssemblyResolver.TestCreate(arguments, null);
            testResolver.AssembliesToResolve.Should().BeEquivalentTo("foo", "bar");
            testResolver.ResolutionPaths.Should().BeEquivalentTo(@"C:\TestLoad1", @"C:\TestLoad2");
        }

        [Fact]
        public void NullOrEmptyRequestReturnsNull()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            TestAssemblyResolver testResolver = TestAssemblyResolver.TestCreate(arguments, null);
            ((object)testResolver.Domain_AssemblyResolve(null, new ResolveEventArgs(null))).Should().BeNull();
            ((object)testResolver.Domain_AssemblyResolve(null, new ResolveEventArgs(string.Empty))).Should().BeNull();
        }

        [Fact]
        public void NoSettingsRequestReturnsNull()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            TestAssemblyResolver testResolver = TestAssemblyResolver.TestCreate(arguments, null);
            ((object)testResolver.Domain_AssemblyResolve(null, new ResolveEventArgs("MyDependency"))).Should().BeNull();
        }

        [Fact]
        public void FoundAssemblyLoads()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>(StandardOptions.AssembliesToResolve).Returns("MyDependency");
            arguments.GetOption<string>(StandardOptions.AssemblyResolutionPaths).Returns(@"C:\TestLoad1");

            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetAttributes(@"C:\TestLoad1\MyDependency.dll").Returns(FileAttributes.Normal);

            TestAssemblyResolver testResolver = Substitute.ForPartsOf<TestAssemblyResolver>(fileService);
            testResolver.TestInitialize(arguments);

            ((object)testResolver.Domain_AssemblyResolve(null, new ResolveEventArgs("MyDependency"))).Should().Be(Assembly.GetExecutingAssembly());
            testResolver.Received(1).TestLoadAssemblyFrom(@"C:\TestLoad1\MyDependency.dll");
        }

        [Fact]
        public void NotFoundAssemblyDoesNotLoad()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            arguments.GetOption<string>(StandardOptions.AssembliesToResolve).Returns("MyDependency");
            arguments.GetOption<string>(StandardOptions.AssemblyResolutionPaths).Returns(@"C:\TestLoad1");

            IFileService fileService = Substitute.For<IFileService>();
            fileService.GetAttributes(@"C:\TestLoad1\MyDependency.dll").Returns(x => { throw new FileNotFoundException(); });

            TestAssemblyResolver testResolver = Substitute.ForPartsOf<TestAssemblyResolver>(fileService);
            testResolver.TestInitialize(arguments);

            ((object)testResolver.Domain_AssemblyResolve(null, new ResolveEventArgs("MyDependency"))).Should().BeNull();
            testResolver.ReceivedWithAnyArgs(0).TestLoadAssemblyFrom("");
        }

        [Fact]
        public void FallbackResolverCalled()
        {
            IArgumentProvider arguments = Substitute.For<IArgumentProvider>();
            IFileService fileService = Substitute.For<IFileService>();
            ResolveEventArgs resolveArgs = new("MyDependency");

            TestAssemblyResolver testResolver = Substitute.ForPartsOf<TestAssemblyResolver>(fileService);
            testResolver.TestInitialize(arguments);

            ((object)testResolver.Domain_AssemblyResolve(null, resolveArgs)).Should().BeNull();
            testResolver.Received(1).TestFallBack_AssemblyResolve(testResolver, resolveArgs);
        }
    }
}
