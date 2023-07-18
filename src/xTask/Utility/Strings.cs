// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace XTask.Utility;

public static class Strings
{
    // Full format specifiers look like this: { index[,alignment][ :formatString] }
    //  Alignment, when specified, will pad spaces on the left (right aligned, positive) or right (left aligned, negative)

    private static readonly Regex s_NewLineRegex = new(
        @"(\r\n|[\r\n]){1,}",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex s_WhiteSpaceRegex = new(
        @"\s{1,}|\p{C}{1,}",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    ///  Trims whitespace, shrinks space runs and control characters to a single space
    /// </summary>
    public static string CompressWhiteSpace(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return s_WhiteSpaceRegex.Replace(value, " ").Trim();
    }

    /// <summary>
    ///  Replaces line ends with the given string.
    /// </summary>
    public static string ReplaceLineEnds(string value, string replacement)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return s_NewLineRegex.Replace(value, replacement);
    }

    /// <summary>
    ///  Replaces tabs with spaces.
    /// </summary>
    public static string TabsToSpaces(string value)
    {
        if (string.IsNullOrEmpty(value)) { return string.Empty; }

        return value.Replace("\t", "   ");
    }

    /// <summary>
    ///  Finds the largest in-common starting string (ordinal comparison).
    /// </summary>
    public static string FindLeftmostCommonString(params string[] values)
    {
        if (values is null || values.Length == 0) { return string.Empty; }

        string leftmost = null;
        foreach (string value in values)
        {
            if (string.IsNullOrEmpty(value)) { return string.Empty; }

            if (leftmost is null)
            {
                leftmost = value;
                continue;
            }

            int i = 0;
            while (i < leftmost.Length && i < value.Length && leftmost[i] == value[i])
            {
                i++;
            }

            if (i == 0)
            {
                return string.Empty;
            }
            else
            {
                leftmost = leftmost.Substring(0, i);
            }
        }

        return leftmost;
    }

    /// <summary>
    ///  Finds the largest in-common ending string (ordinal comparison).
    /// </summary>
    public static string FindRightmostCommonString(params string[] values)
    {
        if (values is null || values.Length == 0) { return string.Empty; }

        string rightmost = null;
        foreach (string value in values)
        {
            if (string.IsNullOrEmpty(value)) { return string.Empty; }

            if (rightmost is null)
            {
                rightmost = value;
                continue;
            }

            int i = 1;
            while (i <= rightmost.Length && i <= value.Length && rightmost[rightmost.Length - i] == value[value.Length - i])
            {
                i++;
            }

            if (i == 1)
            {
                return string.Empty;
            }
            else
            {
                rightmost = rightmost.Substring(rightmost.Length - i + 1);
            }
        }

        return rightmost;
    }

    /// <summary>
    ///  Returns the value if one exists, or "No Value" text if blank.
    /// </summary>
    public static string ValueOrNone(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? XTaskStrings.NoValue : value;
    }

    /// <summary>
    ///  Returns true if the strings are equal, or both are equally "empty".
    /// </summary>
    public static bool EqualsOrNone(string a, string b, StringComparison comparisionType = StringComparison.Ordinal)
    {
        if (string.IsNullOrWhiteSpace(a)) return string.IsNullOrWhiteSpace(b);
        return string.Equals(a, b, comparisionType);
    }

    /// <summary>
    ///  "Underlines" the given string (by creating a line break and underline characters).
    /// </summary>
    /// <param name="breakCharacter">
    ///  Character to treat as a break space, if desired. (To create a "words only" underline style)
    /// </param>
    public static string Underline(string value, char underlineCharacter = '-', char? breakCharacter = '_')
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        int length = value.Length;
        StringBuilder message = new(length * 2 + 2);   // 2 for crlf
        message.AppendLine(value);
        message.Append(underlineCharacter, length);

        // If a break character was specified, use it
        if (breakCharacter.HasValue)
        {
            for (int i = 0; i < length; i++)
            {
                if (message[i] == breakCharacter)
                {
                    message[i] = ' ';
                    message[i + length + 2] = ' ';
                }
            }
        }

        return message.ToString();
    }

    /// <summary>
    ///  Splits a command line by spaces, keeping quoted text together.
    /// </summary>
    public static IEnumerable<string> SplitCommandLine(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) return Enumerable.Empty<string>();
        var args = new List<string>();
        StringBuilder sb = new(commandLine);
        sb.Trim();

        int argStart = 0;
        int lastQuote = -1;
        for (int i = 0; i < sb.Length; i++)
        {
            char current = sb[i];
            if (char.IsWhiteSpace(current))
            {
                if (argStart == i)
                {
                    // Leading argument whitespace
                    argStart++;
                    continue;
                }
                else if (argStart != lastQuote)
                {
                    // Not in a quote, end of arg
                    args.Add(sb.ToString(argStart, i - argStart));
                    argStart = i + 1;
                    continue;
                }
            }

            if (current == '"')
            {
                if (lastQuote == -1)
                {
                    if (argStart == i)
                    {
                        // Start of quote
                        lastQuote = i;
                        argStart = i;
                        continue;
                    }
                }
                else
                {
                    // End of quote, trim out quotes
                    argStart++;
                    args.Add(sb.ToString(argStart, i - argStart));
                    lastQuote = -1;
                    argStart = i + 1;
                    continue;
                }
            }
        }

        if (lastQuote != -1)
        {
            // Orphaned quote, move the argument start forward
            argStart++;
        }

        if (argStart < sb.Length)
        {
            args.Add(sb.ToString(argStart, sb.Length - argStart));
        }

        return args;
    }

    /// <summary>
    ///  Splits an unmanaged character array.
    /// </summary>
    /// <param name="pointer">Pointer to an buffer of Unicode (UTF-16/WCHAR) characters</param>
    /// <param name="length">Length of the buffer in characters</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name=nameof(pointer)/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name=nameof(length)/> is less than zero.</exception>
    unsafe internal static IEnumerable<string> Split(IntPtr pointer, int length, params char[] splitCharacters)
    {
        if (pointer == IntPtr.Zero) throw new ArgumentNullException(nameof(pointer));
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (splitCharacters is null || splitCharacters.Length == 0) return Enumerable.Empty<string>();

        var strings = new List<string>();
        char* start = (char*)pointer;
        char* current = start;

        int stringStart = 0;
        for (int i = 0; i < length; i++)
        {
            if (splitCharacters.Contains(*current))
            {
                // Split
                strings.Add(new string(start, stringStart, i - stringStart));
                stringStart = i + 1;
            }

            current += 1;
        }

        if (stringStart <= length)
        {
            strings.Add(new string(start, stringStart, length - stringStart));
        }

        return strings;
    }

    /// <summary>
    ///  Simple wrapper for String.Compare to compare the beginning of strings.
    /// </summary>
    public static bool StartsWithCount(string first, string second, int count, StringComparison comparisonType)
    {
        return string.Compare(first, 0, second, 0, count, comparisonType) == 0;
    }

    /// <summary>
    ///  Get the count of characters that match at the given indexes walking backwards.
    /// </summary>
    public static int FindRightmostCommonCount(
        string first,
        int firstIndex,
        string second,
        int secondIndex,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (second is null) throw new ArgumentNullException(nameof(second));
        if (firstIndex < 0 || first.Length - firstIndex < 0) throw new ArgumentOutOfRangeException(nameof(firstIndex));
        if (secondIndex < 0 || second.Length - secondIndex < 0) throw new ArgumentOutOfRangeException(nameof(secondIndex));

        if (first.Length == 0
            || second.Length == 0
            || firstIndex == first.Length
            || secondIndex == second.Length)
            return 0;

        int matchCount = 0;
        while (string.Compare(first, firstIndex, second, secondIndex, 1, comparisonType) == 0)
        {
            matchCount++;
            if (firstIndex == 0 || secondIndex == 0) break;
            firstIndex--;
            secondIndex--;
        }

        return matchCount;
    }
}