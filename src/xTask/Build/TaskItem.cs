// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Build
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using MSBuildFramework = Microsoft.Build.Framework;

    public class TaskItem : MarshalByRefObject, MSBuildFramework.ITaskItem
    {
        ConcurrentDictionary<string, string> metadata = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary CloneCustomMetadata()
        {
            return new Dictionary<string, string>(this.metadata);
        }

        public void CopyMetadataTo(MSBuildFramework.ITaskItem destinationItem)
        {
            foreach (var keyPair in this.metadata.ToArray())
            {
                destinationItem.SetMetadata(keyPair.Key, keyPair.Value);
            }
        }

        public string GetMetadata(string metadataName)
        {
            string value;
            this.metadata.TryGetValue(metadataName, out value);
            return value;
        }

        public string ItemSpec { get; set; }

        public int MetadataCount
        {
            get { return this.metadata.Count; }
        }

        public ICollection MetadataNames
        {
            get { return this.metadata.Keys.ToArray(); }
        }

        public void RemoveMetadata(string metadataName)
        {
            string value;
            this.metadata.TryRemove(metadataName, out value);
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            this.metadata.AddOrUpdate(metadataName, metadataValue, (key, existing) => metadataValue);
        }
    }
}