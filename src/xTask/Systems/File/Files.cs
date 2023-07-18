// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XTask.Utility;

namespace XTask.Systems.File;

public static class Files
{
    private static readonly MD5 s_MD5 = MD5.Create();

    /// <summary>
    ///  Finds all files in a given set of paths that contain the specified string.
    /// </summary>
    /// <param name="value">The string to search for</param>
    /// <param name="ignoreCase">True to ignore case</param>
    /// <param name="paths">Paths to search</param>
    public static IEnumerable<string> ContainsString(this IFileService fileService, string value, bool ignoreCase, params string[] paths)
    {
        return ContainsRegex(fileService, Regex.Escape(value), ignoreCase, paths);
    }

    /// <summary>
    ///  Finds all files in a given set of paths that contain the specified regex.
    /// </summary>
    /// <param name="regex">The string to search for</param>
    /// <param name="ignoreCase">True to ignore case</param>
    /// <param name="paths">Paths to search</param>
    public static IEnumerable<string> ContainsRegex(this IFileService fileService, string regex, bool ignoreCase, params string[] paths)
    {
        RegexOptions options = RegexOptions.Compiled;
        if (ignoreCase) { options |= RegexOptions.IgnoreCase; }

        Regex r;
        try
        {
            r = new Regex(regex, options);
        }
        catch (ArgumentException exception)
        {
            // Bad regex
            throw new TaskArgumentException(exception.Message);
        }

        ConcurrentBag<string> matchingPaths = new();

        Parallel.ForEach(paths, path =>
        {
            if (!fileService.FileExists(path)) { return; }

            Stream stream = fileService.CreateFileStream(path);
            using StreamReader reader = new(stream);
            string line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (r.IsMatch(line))
                {
                    matchingPaths.Add(path);
                    break;
                }
            }
        });

        return matchingPaths.OrderBy(s => s).ToList();
    }

    /// <summary>
    ///  Simple line counter for a file, no error handling.
    /// </summary>
    public static int CountLines(this IFileService fileService, string path)
    {
        int lineCount = 0;
        bool trailingCharacter = true;

        // 8K buffer
        char[] buffer = new char[8 * 1024];

        Stream stream = fileService.CreateFileStream(path);
        using (StreamReader reader = new(stream))
        {
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    if (buffer[i] == '\n')
                    {
                        trailingCharacter = false;
                        lineCount++;
                    }
                    else
                    {
                        trailingCharacter = true;
                    }
                }
            }

            if (trailingCharacter) { lineCount++; }
        }

        return lineCount;
    }

    /// <summary>
    ///  Reads lines from the given path.
    /// </summary>
    public static IEnumerable<string> ReadLines(this IFileService fileService, string path)
    {
        Stream stream = fileService.CreateFileStream(path);

        using StreamReader reader = new(stream);
        string line = reader.ReadLine();

        while (line is not null)
        {
            yield return line;
            line = reader.ReadLine();
        }
    }

    /// <summary>
    ///  Attempts to delete the given file, returns error message if unsuccessful.
    /// </summary>
    public static string TryDelete(this IFileService fileService, string file)
    {
        try
        {
            fileService.MakeWritable(file);
            fileService.DeleteFile(file);
            return null;
        }
        catch (Exception e)
        {
            // Don't fail if we can't delete for any reason
            return e.Message;
        }
    }

    /// <summary>
    ///  Returns the MD5 hash for the given file's contents, or <see langword="null"/> if failed.
    /// </summary>
    public static byte[] GetHash(this IFileService fileService, string path)
    {
        try
        {
            // Console.WriteLine("Hashing {0}...", localPath);
            if (!string.IsNullOrEmpty(path) & fileService.FileExists(path))
            {
                using Stream fileStream = fileService.CreateFileStream
                    (path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return s_MD5.ComputeHash(fileStream);
            }
        }
        catch (IOException)
        {
            // If there is an I/O problem, do nothing and allow this method to return null.
        }

        return null;
    }
}