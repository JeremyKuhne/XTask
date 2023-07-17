// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace XTask.Utility
{
    /// <summary>
    ///  Simple helper to escape / unescape XML strings
    /// </summary>
    public static class XmlEscaper
    {
        public static string Escape(string xmlString)
        {
            // This will only create entities for the standard SGML/XML five: (&lt; &gt; &amp; &apos; and &quot;)
            return WebUtility.HtmlEncode(xmlString);
        }

        public static string Unescape(string escapedString)
        {
            // This isn't technically correct, as it will recognize a greater set of character entities than the core
            // (&lt; &gt; &amp; &apos; and &quot;), but entities are entities- decoding html standard defined entities
            // such as &reg; could be considered a feature, if anything.
            return WebUtility.HtmlDecode(escapedString);
        }
    }
}
