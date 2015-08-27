// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using System.IO;

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
                fullPath = Path.Combine(fullPath, directoryName);
            }

            if (lastDirectory > 0)
            {
                fullPath = Path.Combine(fullPath, directoryName.Substring(0, lastDirectory));
            }

            return fullPath;
        }
    }
}
