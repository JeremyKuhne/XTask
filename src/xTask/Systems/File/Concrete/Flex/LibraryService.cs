// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.Flex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Interop;

    public class LibraryService
    {
        public SafeLibraryHandle LoadLibrary(string path, LoadLibraryFlags flags)
        {
            return NativeMethods.LoadLibrary(path, flags);
        }

        public string LoadString(SafeLibraryHandle library, int identifier)
        {
            return NativeMethods.LoadString(library, identifier);
        }

        public DelegateType GetFunctionDelegate<DelegateType>(SafeLibraryHandle library, string methodName)
        {
            return NativeMethods.GetFunctionDelegate<DelegateType>(library, methodName);
        }
    }
}
