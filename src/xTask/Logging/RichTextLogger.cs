// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace XTask.Logging;

public class RichTextLogger : Logger, IClipboardSource
{
    // Standard typography measurements:
    //
    //  pica is 1/6 of an inch (12 point)
    //  point is 1/12 of a pica (1/72 inch)
    //  twip is 1/20 of a point (1/1440 inch)

    private const int FontSize = 11;
    private const string DoubleFontSize = "22";
    private const int TwipsPerPoint = 20;

    protected StringBuilder _richText = new(4096);
    private readonly StringBuilder _escaper = new();
    private bool _priorControlCharacter = true;

    protected override void WriteInternal(WriteStyle style, string value)
    {
        bool bold = style.HasFlag(WriteStyle.Bold);
        bool underline = style.HasFlag(WriteStyle.Underline);
        bool italic = style.HasFlag(WriteStyle.Italic);
        bool fixedWidth = style.HasFlag(WriteStyle.Fixed);

        // 1. Output Style start tags (if any) \f1 \ul \b \i
        // 2. Output escaped text
        // 3. Output Style end tags \f0 \ul0 \b0 \i0
        if (bold) { _richText.Append(@"\b"); }
        if (fixedWidth) { _richText.Append(@"\f1"); }
        if (underline) { _richText.Append(@"\ul"); }
        if (italic) { _richText.Append(@"\i"); }
        _priorControlCharacter |= bold || italic || underline;

        _richText.Append(Escape(value));

        if (bold) { _richText.Append(@"\b0"); }
        if (fixedWidth) { _richText.Append(@"\f0"); }
        if (underline) { _richText.Append(@"\ul0"); }
        if (italic) { _richText.Append(@"\i0"); }
        _priorControlCharacter |= bold || italic || underline;

        // Perhaps bracket every Write to scope style? Then resetting fonts isn't necessary {}
    }

    protected string Escape(string value)
    {
        // 1. Escape '\\', '{', and '}'
        // 2. Escape non-ASCII unicode (to \'hh)
        // 3. Escape '\t' to \tab and '\n' to \par

        _escaper.Clear();
        foreach (char c in value)
        {
            if ((uint)c > 128)
            {
                // Unicode outside of the ASCII space- needs escaping

                // NOTES:
                // There are a variety of ways of handling this, including switching the code page to one that
                // contains the character.  For example キ could be set to shift-jis \lang9 and output as \' hex bytes.
                // It is certainly more compact, but I'm not sure where to find the code to do this.

                // Substitute codepage character needs to follow the unicode character. We're picking the usual '?'.
                _escaper.AppendFormat(CultureInfo.InvariantCulture, @"\u{0}\'3f", (uint)c);
                // Don't need?
                // this.priorControlCharacter = true;
            }
            else
            {
                // ASCII compatible character- escape special characters but otherwise add as is
                switch (c)
                {
                    case '\\':
                    case '{':
                    case '}':
                        // These have special meaning in RTF, need to escape
                        _escaper.Append('\\');
                        _escaper.Append(c);
                        _priorControlCharacter = false;
                        break;
                    case '\n':
                        _escaper.Append(@"\par");
                        _priorControlCharacter = true;
                        break;
                    case '\t':
                        _escaper.Append(@"\tab");
                        _priorControlCharacter = true;
                        break;
                    default:
                        if (_priorControlCharacter)
                        {
                            _escaper.Append(' ');
                            _priorControlCharacter = false;
                        }
                        _escaper.Append(c);
                        break;
                }
            }
        }

        return _escaper.ToString();
    }

    // Font Colors:
    // ============
    // Red-    \red255\green0\blue0;
    // Orange- \red255\green192\blue0;
    // Yellow- \red255\green255\blue0;
    // Green-  \red0\green176\blue80;
    // Blue-   \red0\green77\blue187;
    // Purple- \red155\green0\blue211;

    // Highlight Colors:
    // =================
    // Yellow-    \red255\green255\blue0;
    // Green-     \red0\green255\blue0;
    // LightBlue- \red0\green255\blue255;
    // Pink-      \red255\green0\blue255;

    // {\colortbl ;}
    // \highlight1/0
    // \cf1/0

    public override void Write(ITable table)
    {
        // <row>    (<tbldef> <cell>+ <tbldef> \row) | (<tbldef> <cell>+ \row) | (<cell>+ <tbldef> \row)
        // <cell>   (<nestrow>? <tbldef>?) & <textpar>+ \cell

        //  12 point font
        //  6 point padding  (120 twips)
        //  14 point height  (280 twips)
        //  6.5" total width (9360 twips)

        // Calculate widths (default 1n), converting to fractions of 6.5" in twips (9360)
        int[] columnWidths = ColumnFormat.ScaleColumnWidths(9360, table.ColumnFormats);

        // Create the row header
        //   \trowd        // Start row
        //   \trgaph120    // Half space between cells in twips (6pt)
        //   \trrh280      // Row height in twips (0 auto, + at least, - exact) (14pt)
        //   \trpaddl120   // Cell left padding (6pt)
        //   \trpaddr120   // Cell right padding
        //   \trpaddfl3    // Left padding unit is twips
        //   \trpaddfr3    // Right padding unit is twips
        StringBuilder rowHeader = new(128);

        rowHeader.AppendFormat(@"{{\trowd\trgaph{0}\trrh-{1}\trpaddl{0}\trpaddr{0}\trpaddfl3\trpaddfr3",
            FontSize / 2 * TwipsPerPoint,      // Padding of half the font size
            (FontSize + 2) * TwipsPerPoint);   // Row height, negative to make "exactly"

        int totalWidth = 0;
        for (int i = 0; i < columnWidths.Length; i++)
        {
            totalWidth += columnWidths[i];
            rowHeader.Append(@"\cellx");
            rowHeader.Append(totalWidth);
        }

        bool headerRow = table.HasHeader;
        _priorControlCharacter = false;
        foreach (var row in table.Rows)
        {
            _richText.Append(rowHeader);
            if (headerRow) _richText.Append(@"\b");
            for (int i = 0; i < row.Length; i++)
            {
                string alignment = table.ColumnFormats[i].Justification switch
                {
                    Justification.Centered => @"\qc",
                    Justification.Right => @"\qr",
                    _ => @"\ql",
                };

                _richText.AppendFormat(@"\pard\intbl\widctlpar{0} {1}\cell", alignment, Escape(row[i]));
            }

            if (headerRow)
            {
                _richText.Append(@"\b0");
                headerRow = false;
            }
            _richText.Append(@"\row}");
        }
        _priorControlCharacter = true;
    }

    public void Save(string path)
    {
        File.WriteAllText(path, ToString(), Encoding.ASCII);
    }

    public override string ToString()
    {
        // {Header}{Body}
        // We don't close out the scope in GetHeader(), so we have to close it here (escaped to '}}' for formatting).
        return string.Format(CultureInfo.InvariantCulture, "{0}{1}}}", GetHeader(), _richText.ToString());
    }

    public ClipboardData GetClipboardData()
        => _richText.Length == 0 ? default : new(ToString().AsMemory(), ClipboardFormat.RichText);

    private string GetHeader()
    {
        // Header
        //   \rtf1 \fbidis? <character set> <from>? <deffont> <deflang> <fonttbl>? <filetbl>? <colortbl>?
        //     <stylesheet>? <stylerestrictions>? <listtables>? <revtbl>? <rsidtable>? <mathprops>? <generator>?
        //
        // FontTable
        //   '{' \fonttbl (<fontinfo> | ('{' <fontinfo> '}'))+ '}'
        //
        // FontTableEntry
        //   <themefont>? \fN <fontfamily> \fcharsetN? \fprq? <panose>? <nontaggedname>? <fontemb>? \cpgN? <fontname> <fontaltname>? ';' 

        return
            @"{\rtf1" +               // Header
            @"\ansi" +                // Document character set (Unicode is escaped)
            @"\ansicpg1252" +         // Codepage for converting Unicode to ANSI
            @"\deff0" +               // Default font (from the font table)
            @"\deflang1033" +         // Language to use when resetting font formatting via \plain (bold/italic/underline, font size, etc.)
            // 1033 is US English
            @"{\fonttbl" +            // Define the font table
                @"{\fhiminor" +       // This font is the default body font
                @"\f0" +              // Font id is '0'
                @"\fnil" +            // Default/unknown font family
                @"\fcharset0" +       // Bytes in runs for this font are ANSI code page (1252)
                @" Calibri;}" +       // Font name is Calibri
                @"{\f1" +             // Font id is '1'
                @"\fmodern" +         // Font is fixed width
                @"\fcharset0" +
                @" Consolas;}}" +     // Font name is Consolas

            // Top level scope formatting definitions

            @"\uc1" +                 // When we output Unicode, we'll follow with 1 replacement codepage character
            @"\pard" +                // Reset paragraph formatting to default
            @"\sl0" +                 // Auto line spacing
            @"\slmult1" +             // \sl is relative to "Single" line spacing (must follow \sl)
            @"\fs" + DoubleFontSize;  // Font size in half points
    }

    //   Paragraph and Spacing
    //   ---------------------
    //   \par new paragraph (new line)
    //   \pard reset to default paragraph properties
    //   \sa \sb  space before and after in twips (default 0)
    //   \sl space between lines
    //       \sl0 is auto positive is used if taller than the tallest char, negative the absolute value is used, even if not taller
    //       \slmult modifies \sl to be 0 - "At Least" or "Exactly" or 1 - Multiple line spacing, relative to "Single"
    //   \qc  Centered
    //   \qj  Justified
    //   \ql  Left-aligned
    //   \qr  Right-aligned
    //   \qd  Distributed
    //
    //   Character Formatting
    //   --------------------
    //   \b \b0 bold on & off
    //   \i \i0 italic on & off
    //   \ul \ul0 continuous underlining on & off
    //
    //   \cb \cf background and foreground color from color table
    //   \f font number in the font table
    //   \fs font size in half points (24 is default)
    //
    //   \tab
    //
    //   \'hh hex character
}
