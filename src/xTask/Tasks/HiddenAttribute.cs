// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace XTask.Tasks
{
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class HiddenAttribute : Attribute
    {
    }
}
