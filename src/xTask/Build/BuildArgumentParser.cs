// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Xml;
using XTask.Systems.File;
using XTask.Settings;

namespace XTask.Build
{
    /// <summary>
    ///  Argument parser for Microsoft Build formatted arguments.
    /// </summary>
    public class BuildArgumentParser : ArgumentProvider
    {
        public BuildArgumentParser(string taskName, string[] targets, string options, IFileService fileService)
            : base (fileService)
        {
            AddTarget(taskName);
            if (targets is not null)
            {
                foreach (string target in targets)
                    AddTarget(target);
            }

            if (string.IsNullOrWhiteSpace(options)) { return; }

            // Options are in xml format <Option>value</Option> or <Option/> for default
            XmlReaderSettings settings = new()
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using XmlReader reader = XmlReader.Create(new StringReader(options), settings);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    AddOrUpdateOption(
                        optionName: reader.Name,
                        optionValue: reader.ReadString());
                }
            }
        }
    }
}