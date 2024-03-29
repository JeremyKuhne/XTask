﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Utility;

public class FileContentComparer : IEqualityComparer<IFileInformation>
{
    public bool Equals(IFileInformation x, IFileInformation y)
    {
        if (x is null || y is null) { return x == y; }

        if (x.Length == y.Length && Arrays.AreEquivalent(x.MD5Hash, y.MD5Hash))
        {
            return true;
        }

        return false;
    }

    public int GetHashCode(IFileInformation obj) => obj.Path.GetHashCode();
}
