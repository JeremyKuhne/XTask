// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    public static class StandardOptions
    {
        // Consider other similar tools and internal consistency when defining options
        //
        // Powershell common parameters
        //  Debug (db)      ErrorAction (ea)   ErrorVariable (ev)    OutVariable (ov)
        //  OutBuffer (ob)  Verbose (vb)       WarningAction (wa)    WarningVariable (wv)
        //
        //  Confirm (cf)    WhatIf (wi)

        public static readonly string[] Add = { "add" };
        public static readonly string[] All = { "all" };
        public static readonly string[] AssemblyResolutionPaths = { "assemblyResolutionPaths", "arp" };
        public static readonly string[] AssembliesToResolve = { "assembliesToResolve", "atr" };
        public static readonly string[] Clipboard = { "clipboard", "clip" };
        public static readonly string[] Confirm = { "confirm", "cf" };
        public static readonly string[] Comment = { "comment", "cmt" };
        public static readonly string[] Contains = { "contains", "cont" };
        public static readonly string[] Count = { "count", "c", "stopAfter" };
        public static readonly string[] Days = { "days" };
        public static readonly string[] Delete = { "delete", "del" };
        public static readonly string[] Difference = { "difference", "diff" };
        public static readonly string[] Directories = { "directories", "dir" };
        public static readonly string[] ExcludeDirectories = { "excludeDirectories", "xdir" };
        public static readonly string[] ExcludeExtensions = { "excludeExtensions", "xext" };
        public static readonly string[] ExcludeFiles = { "excludeFiles", "xfiles" };
        public static readonly string[] Extended = { "extended" };
        public static readonly string[] Files = { "files" };
        public static readonly string[] Force = { "force", "f" };
        public static readonly string[] Hash = { "hash" };
        public static readonly string[] Help = { "help", "?" };
        public static readonly string[] Hours = { "hours" };
        public static readonly string[] IgnoreCase = { "ignoreCase", "ic" };
        public static readonly string[] IgnoreWhiteSpace = { "ignoreWhiteSpace", "iws", "iw" };
        public static readonly string[] IgnoreLeadingAndTrailing = { "ignoreLeadingAndTrailingWhiteSpace", "ignoreLeadingAndTrailing", "iltws", "ilt" };
        public static readonly string[] IgnoreEndOfLine = { "ignoreEndOfLineDifference", "ignoreEndOfLine", "ieold", "ieol" };
        public static readonly string[] IgnoreEndOfFile = { "ignoreEndOfFileEndOfLineDifference", "ignoreEndOfFile", "ieofold", "ieofeol", "ieof" };
        public static readonly string[] Latest = { "latest" };
        public static readonly string[] List = { "list" };
        public static readonly string[] Minutes = { "minutes", "mins" };
        public static readonly string[] NoGui = { "noGui" };
        public static readonly string[] NoPrompt = { "noPrompt", "np" };
        public static readonly string[] Overwrite = { "overwrite", "ow" };
        public static readonly string[] Recursive = { "recursive", "r" };
        public static readonly string[] Regex = { "regex" };
        public static readonly string[] Remove = { "remove" };
        public static readonly string[] Restore = { "restore", "rest" };
        public static readonly string[] Save = { "save" };
        public static readonly string[] ShowFiles = { "showFiles", "sf" };
        public static readonly string[] User = { "user", "u" };
        public static readonly string[] Version = { "version", "v" };
        public static readonly string[] Weeks = { "weeks" };
        public static readonly string[] WhatIf = { "whatIf", "preview", "wi" };
        public static readonly string[] Writable = { "writable" };
    }
}