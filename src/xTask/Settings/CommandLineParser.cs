// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System;
    using Systems.File;

    /// <summary>
    /// Standard command line argument parser.
    /// </summary>
    public class CommandLineParser : ArgumentProvider
    {
        public CommandLineParser(IFileService fileService)
            : base (fileService)
        {
        }

        public void Parse(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0) { return; }

            for (int i = 0; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                if (String.IsNullOrWhiteSpace(argument)) { continue; }

                argument = argument.Trim();

                switch (argument[0])
                {
                    case '/':
                    case '-':
                        argument = argument.Substring(1).Trim();
                        int colonIndex = argument.IndexOf(':');
                        if (colonIndex == -1)
                        {
                            this.AddOrUpdateOption(argument);
                        }
                        else
                        {
                            this.AddOrUpdateOption(argument.Substring(0, colonIndex), argument.Substring(colonIndex + 1));
                        }
                        break;
                    default:
                        this.AddTarget(argument);
                        break;
                }
            }
        }
    }
}