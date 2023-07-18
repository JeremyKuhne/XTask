// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Text;
using XTask.Utility;

namespace XTask.Logging;

// Before we start using this we would want it to match the RTF formatter exactly.
// The "rich" logging we're looking for is for Wordpad, Word, Outlook.
public class HtmlLogger : Logger, IClipboardSource
{
    protected StringBuilder _htmlText = new(4096);
    private readonly int _initialLength;

    public HtmlLogger()
    {
        _htmlText.Append(@"<div style='font-size:11.0pt;font-family:Calibri,sans-serif>");
        _initialLength = _htmlText.Length;
    }

    protected override void WriteInternal(WriteStyle style, string value)
    {
        _htmlText.AppendFormat(
            @"<span style='font-size:11.0pt;font-family:Calibri,sans-serif;white-space:pre{0}'>",
            style.HasFlag(WriteStyle.Error) ? @";color:red" : string.Empty);

        AppendFormatedString(style, value);

        _htmlText.Append(@"</span>");
    }

    protected void AppendFormatedString(WriteStyle style, string value)
    {
        if (style.HasFlag(WriteStyle.Bold)) _htmlText.Append(@"<b>");
        if (style.HasFlag(WriteStyle.Italic)) _htmlText.Append(@"<i>");
        if (style.HasFlag(WriteStyle.Critical)) _htmlText.Append(@"<strong>");
        if (style.HasFlag(WriteStyle.Important)) _htmlText.Append(@"<em>");
        if (style.HasFlag(WriteStyle.Underline)) _htmlText.Append(@"<u>");
        if (style.HasFlag(WriteStyle.Fixed)) _htmlText.Append(@"<pre>"); ;
        _htmlText.Append(Strings.ReplaceLineEnds(WebUtility.HtmlEncode(value), @"<br>"));
        if (style.HasFlag(WriteStyle.Fixed)) _htmlText.Append(@"</pre>"); ;
        if (style.HasFlag(WriteStyle.Underline)) _htmlText.Append(@"</u>");
        if (style.HasFlag(WriteStyle.Important)) _htmlText.Append(@"</em>");
        if (style.HasFlag(WriteStyle.Critical)) _htmlText.Append(@"</strong>");
        if (style.HasFlag(WriteStyle.Italic)) _htmlText.Append(@"</i>");
        if (style.HasFlag(WriteStyle.Bold)) _htmlText.Append(@"</b>");
    }

    public override void Write(ITable table)
    {
        _htmlText.Append(@"<table style='border-collapse:collapse' border=0 cellspacing=0 cellpadding=0>");
        bool headerRow = table.HasHeader;
        foreach (var row in table.Rows)
        {
            _htmlText.Append(@"<tr>");
            for (int i = 0; i < row.Length; i++)
            {
                _htmlText.Append(headerRow ? @"<th" : @"<td");

                switch (table.ColumnFormats[i].Justification)
                {
                    case Justification.Centered:
                        _htmlText.Append(@" style='text-align:center'>");
                        break;
                    case Justification.Right:
                        _htmlText.Append(@" style='text-align:right'>");
                        break;
                    case Justification.Left:
                    default:
                        _htmlText.Append(@" style='text-align:left'>");
                        break;
                }

                _htmlText.AppendFormat("<span style='font-size:11.0pt;font-family:Calibri,sans-serif;white-space:pre'>{0}</span>", row[i]);
                _htmlText.Append(headerRow ? @"</th>" : @"</td>");
            }

            headerRow = false;
            _htmlText.Append(@"</tr>");
        }
        _htmlText.Append(@"</table>");
    }

    public override string ToString()
    {
        return _htmlText.ToString() + @"</div>";
    }

    public ClipboardData GetClipboardData()
        => _htmlText.Length == _initialLength
            ? default
            : new(FormatForClipboard(ToString()).AsMemory(), ClipboardFormat.Html);

    // Adapted from Mike Stall's MSDN Blog:
    // http://blogs.msdn.com/b/jmstall/archive/2007/01/21/sample-code-html-clipboard.aspx

    /// <summary>
    ///  Wraps an html fragment in the HTML clipboard format.
    /// </summary>
    /// <param name="htmlFragment">a html fragment</param>
    /// <param name="title">optional title of the HTML document (can be null)</param>
    /// <param name="sourceUrl">optional Source URL of the HTML document, for resolving relative links (can be null)</param>
    protected static string FormatForClipboard(string htmlFragment, string title = null, Uri sourceUrl = null)
    {
        title ??= "From Clipboard";

        StringBuilder sb = new();

        // Builds the CF_HTML header. See the format specification here:
        // http://msdn.microsoft.com/library/aa767917.aspx
        // http://msdn.microsoft.com/library/windows/desktop/ms649015.aspx

        // 10 characters is enough for a GB on the clipboard
        sb.AppendLine("Format:HTML Format");
        sb.AppendLine("Version:1.0");
        sb.AppendLine(    "StartHTML:START_HTML");
        sb.AppendLine(      "EndHTML:__END_HTML");
        sb.AppendLine("StartFragment:START_FRAG");
        sb.AppendLine(  "EndFragment:__END_FRAG");

        int headerLength = sb.Length;

        // Optional
        // sb.AppendLine("StartSelection:0000000000");
        // sb.AppendLine("EndSelection:0000000000");

        if (sourceUrl is not null)
        {
            sb.AppendFormat("SourceURL:{0}", sourceUrl);
        }

        sb.Replace("START_HTML", sb.Length.ToString("D10"), startIndex: 0, count: headerLength);

        sb.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN""><HTML><HEAD><TITLE>");
        sb.Append(title);
        sb.Append(@"</TITLE></HEAD><BODY><!--StartFragment-->");
        sb.Replace("START_FRAG", sb.Length.ToString("D10"), startIndex: 0, count: headerLength);

        sb.Append(htmlFragment);
        sb.Replace("__END_FRAG", sb.Length.ToString("D10"), startIndex: 0, count: headerLength);

        sb.Append(@"<!--EndFragment--></BODY></HTML>");
        sb.Replace("__END_HTML", sb.Length.ToString("D10"), startIndex: 0, count: headerLength);

        return sb.ToString();
    }
}
