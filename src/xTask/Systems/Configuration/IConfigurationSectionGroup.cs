﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Configuration;

namespace XTask.Systems.Configuration;

public interface IConfigurationSectionGroup
{
    ClientSettingsSection Get(string name);
    void Add(string name, ClientSettingsSection value);
}
