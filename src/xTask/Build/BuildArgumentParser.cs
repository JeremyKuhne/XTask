﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Build
{
    using System;
    using System.IO;
    using System.Xml;
    using Systems.File;
    using XTask.Settings;

    /// <summary>
    /// Argument parser for Microsoft Build formatted arguments
    /// </summary>
    public class BuildArgumentParser : ArgumentProvider
    {
        public BuildArgumentParser(string taskName, string[] targets, string options, IFileService fileService)
            : base (fileService)
        {
            AddTarget(taskName);
            if (targets != null)
            {
                foreach (string target in targets)
                    AddTarget(target);
            }

            if (string.IsNullOrWhiteSpace(options)) { return; }

            // Options are in xml format <Option>value</Option> or <Option/> for default
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using (XmlReader reader = XmlReader.Create(new StringReader(options), settings))
            {
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
}