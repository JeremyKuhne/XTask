// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32;
using Windows.Win32.Foundation;
using xTask.Utility;

namespace XTask.Systems.File.Concrete.Flex;

/// <summary>
///  Maintains a set of current directories for all volumes. Normalizes volume names.
/// </summary>
public class CurrentDirectory
{
    private readonly IExtendedFileService _extendedFileService;
    private readonly IFileService _fileService;
    private readonly Dictionary<string, string> _volumeDirectories = new();
    private string _lastVolume;

    public CurrentDirectory(IFileService fileService, IExtendedFileService extendedFileService, string initialCurrentDirectory = null)
    {
        _fileService = fileService;
        _extendedFileService = extendedFileService;
        SetCurrentDirectory(initialCurrentDirectory ?? Environment.CurrentDirectory);
    }

    /// <summary>
    ///  Sets the current directory.
    /// </summary>
    /// <exception cref="ArgumentException">Returned if <paramref name="nameof(directory)"/> isn't fully qualified.</exception>
    public void SetCurrentDirectory(string directory)
    {
        if (Paths.IsPartiallyQualified(directory))
        {
            throw new ArgumentException("Argument cannot be relative", nameof(directory));
        }

        _lastVolume = AddEntry(directory);
    }

    private string AddEntry(string directory, string canonicalRoot = null)
    {
        string root = Paths.GetRoot(directory);
        canonicalRoot ??= _extendedFileService.GetCanonicalRoot(_fileService, directory);

        // Look for the highest level existing directory or use the root
        while (!_fileService.DirectoryExists(directory)
            && !string.Equals((directory = Paths.GetDirectory(directory)), root, StringComparison.Ordinal))
        {
            Debug.Assert(directory is not null);
        }

        if (_volumeDirectories.ContainsKey(canonicalRoot))
        {
            _volumeDirectories[canonicalRoot] = directory;
        }
        else
        {
            _volumeDirectories.Add(canonicalRoot, directory);
        }

        return canonicalRoot;
    }

    /// <summary>
    ///  Get the current directory for the volume of the given path.
    ///  If no path is given, returns the volume for the last set current directory.
    /// </summary>
    public unsafe string GetCurrentDirectory(string path = null)
    {
        // Find the path's volume or use the last set volume if there is no path
        string volume = path is null ? _lastVolume : _extendedFileService.GetCanonicalRoot(_fileService, path);

        if (_volumeDirectories.TryGetValue(volume, out string directory))
        {
            // We have a current directory from this volume
            AddEntry(directory, volume);
            return directory;
        }

        // No current directory yet for this volume

        // Try to get the hidden environment variable (e.g. "=C:") for the given drive if available
        string driveLetter = _extendedFileService.GetDriveLetter(_fileService, path);
        if (!string.IsNullOrEmpty(driveLetter))
        {
            // Unfortunately these get filtered out by .NET so we have to get it manually.
            string variable = $"={driveLetter[0]}:";
            using BufferScope<char> buffer = new(stackalloc char[256]);
            while (true)
            {
                fixed(char* b = buffer)
                {
                    uint count = Interop.GetEnvironmentVariable(variable, b, (uint)buffer.Length);
                    if (count == 0)
                    {
                        Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_ENVVAR_NOT_FOUND);
                        break;
                    }

                    if (count < buffer.Length)
                    {
                        string environmentPath = buffer.Slice(0, (int)count).ToString();
                        AddEntry(environmentPath);
                        return environmentPath;
                    }

                    buffer.EnsureCapacity((int)count);
                }
            }
        }

        // No stashed environment variable, add the root of the path as our current directory
        string root = Paths.GetRoot(path);
        AddEntry(root);
        return root;
    }
}
