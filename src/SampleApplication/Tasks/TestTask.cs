// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XTask.Interop;
    using XTask.Systems.File;
    using XTask.Systems.File.Concrete.Flex;
    using XTask.Utility;

    [XTask.Tasks.Hidden]
    public class TestTask : FileTask
    {
        internal const string NativeTestLibrary = "NativeTestLibrary.dll";


        protected override ExitCode ExecuteFileTask()
        {
            using (var cleaner = new FileCleaner("TestTask", FileService))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                FileService.CreateDirectory(longPath);
                string longPathLibrary = Paths.Combine(longPath, NativeTestLibrary);
                FileService.CopyFile(NativeTestLibrary, longPathLibrary);
                longPathLibrary = Paths.AddExtendedPrefix(longPathLibrary);

                var libraryService = new LibraryService();

                using (var handle = libraryService.LoadLibrary(longPathLibrary, LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH))
                {
                }
            }

            return ExitCode.Success;
        }
    }

    public static class PathGenerator
    {
        public static string CreatePathOfLength(string root, int totalLength)
        {
            const string directoryName = "1234567890";
            int neededLength = totalLength - root.Length;
            int directoryCount = neededLength / (directoryName.Length + 1);
            int lastDirectory = neededLength % (directoryName.Length + 1) + 1;
            string fullPath = root;
            for (int i = 0; i < directoryCount; i++)
            {
                fullPath = Paths.Combine(fullPath, directoryName);
            }

            if (lastDirectory > 0)
            {
                fullPath = Paths.Combine(fullPath, directoryName.Substring(0, lastDirectory));
            }

            return fullPath;
        }
    }
}
