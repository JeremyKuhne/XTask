// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MSBuildFramework = Microsoft.Build.Framework;

namespace XTask.Build;

public class TaskItem : MarshalByRefObject, MSBuildFramework.ITaskItem
{
    private readonly ConcurrentDictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    public IDictionary CloneCustomMetadata() => new Dictionary<string, string>(_metadata);

    public void CopyMetadataTo(MSBuildFramework.ITaskItem destinationItem)
    {
        foreach (var keyPair in _metadata.ToArray())
        {
            destinationItem.SetMetadata(keyPair.Key, keyPair.Value);
        }
    }

    public string GetMetadata(string metadataName)
    {
        _metadata.TryGetValue(metadataName, out string value);
        return value;
    }

    public string ItemSpec { get; set; }

    public int MetadataCount => _metadata.Count;

    public ICollection MetadataNames => _metadata.Keys.ToArray();

    public void RemoveMetadata(string metadataName) => _metadata.TryRemove(metadataName, out _);

    public void SetMetadata(string metadataName, string metadataValue)
        => _metadata.AddOrUpdate(metadataName, metadataValue, (key, existing) => metadataValue);
}