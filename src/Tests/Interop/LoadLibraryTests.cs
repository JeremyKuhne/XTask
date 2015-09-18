// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using System.Runtime.InteropServices;
    using System.Security;
    using Systems.File;
    using XTask.Interop;
    using XTask.Systems.File.Concrete.Flex;
    using Xunit;

    public class LoadLibraryTests
    {
        internal const string NativeTestLibrary = "NativeTestLibrary.dll";

        [Fact]
        public void LoadAsResource()
        {
            using (var handle = NativeMethods.LoadLibrary(NativeTestLibrary, LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE))
            {
                handle.IsInvalid.Should().BeFalse();
            }
        }

        [Fact]
        public void LoadAsResourceFromLongPath()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                IFileService system = new FileService();
                system.CreateDirectory(longPath);
                string longPathLibrary = Paths.Combine(longPath, NativeTestLibrary);
                system.CopyFile(NativeTestLibrary, longPathLibrary);
                longPathLibrary = Paths.AddExtendedPrefix(longPathLibrary);

                using (var handle = NativeMethods.LoadLibrary(longPathLibrary, LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE))
                {
                    handle.IsInvalid.Should().BeFalse();
                }
            }
        }

        [Fact]
        public void LoadString()
        {
            using (var handle = NativeMethods.LoadLibrary(NativeTestLibrary, LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE))
            {
                string resource = NativeMethods.LoadString(handle, 101);
                resource.Should().Be("Test");
            }
        }

        [Fact]
        public void LoadStringFromLongPath()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                IFileService system = new FileService();
                system.CreateDirectory(longPath);
                string longPathLibrary = Paths.Combine(longPath, NativeTestLibrary);
                system.CopyFile(NativeTestLibrary, longPathLibrary);
                longPathLibrary = Paths.AddExtendedPrefix(longPathLibrary);

                using (var handle = NativeMethods.LoadLibrary(longPathLibrary, LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE))
                {
                    string resource = NativeMethods.LoadString(handle, 101);
                    resource.Should().Be("Test");
                }
            }
        }

        [Fact]
        public void LoadAsBinary()
        {
            using (var handle = NativeMethods.LoadLibrary(NativeTestLibrary, 0))
            {
                handle.IsInvalid.Should().BeFalse();
            }
        }

        [Fact]
        public void LoadAsBinaryFromLongPath()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                IFileService system = new FileService();
                system.CreateDirectory(longPath);
                string longPathLibrary = Paths.Combine(longPath, NativeTestLibrary);
                system.CopyFile(NativeTestLibrary, longPathLibrary);
                longPathLibrary = Paths.AddExtendedPrefix(longPathLibrary);

                using (var handle = NativeMethods.LoadLibrary(longPathLibrary, LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH))
                {
                    handle.IsInvalid.Should().BeFalse();
                }
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private delegate int DoubleDelegate(int value);

        [Fact]
        public void LoadFunction()
        {
            using (var handle = NativeMethods.LoadLibrary(NativeTestLibrary, LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH))
            {
                var doubler = NativeMethods.GetFunctionDelegate<DoubleDelegate>(handle, "Double");
                doubler(2).Should().Be(4);
            }
        }

        [Fact]
        public void LoadFunctionFromLongPath()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                IFileService system = new FileService();
                system.CreateDirectory(longPath);
                string longPathLibrary = Paths.Combine(longPath, NativeTestLibrary);
                system.CopyFile(NativeTestLibrary, longPathLibrary);
                longPathLibrary = Paths.AddExtendedPrefix(longPathLibrary);

                using (var handle = NativeMethods.LoadLibrary(longPathLibrary, 0))
                {
                    var doubler = NativeMethods.GetFunctionDelegate<DoubleDelegate>(handle, "Double");
                    doubler(2).Should().Be(4);
                }
            }
        }

    }
}
