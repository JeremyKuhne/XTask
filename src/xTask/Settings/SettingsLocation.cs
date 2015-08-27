// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    public enum SettingsLocation
    {
        /// <summary>
        /// Local user profile directory (AppData\Local)
        /// </summary>
        Local,

        /// <summary>
        /// Roaming user profile directory (AppData\Roaming)
        /// </summary>
        Roaming,

        /// <summary>
        /// The launching executable config file (App.exe.config)
        /// </summary>
        RunningExecutable,

        /// <summary>
        /// The launching executable config file (App.exe.config)
        /// </summary>
        Executable = RunningExecutable,

        /// <summary>
        /// The launching executable config file (App.exe.config)
        /// </summary>
        ContainingExecutable,

        /// <summary>
        /// The Machine.config file (in the Framework\Config folder)
        /// </summary>
        Machine
    }
}
