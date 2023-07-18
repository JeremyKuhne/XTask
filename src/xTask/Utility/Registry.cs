// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using Win32 = Microsoft.Win32;

namespace XTask.Utility
{
    /// <summary>
    ///  Safe registry access routines.
    /// </summary>
    public class Registry
    {
        // Making the class non-static allows us to derive to test the protected members
        protected Registry() {}

        /// <summary>
        ///  Wraps the superset of RegistryKey exceptions.  Returns the exception it caught or null if none.
        /// </summary>
        protected static Exception RegistryExceptionWrapper(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e is ArgumentException
                    || e is IOException
                    || e is ObjectDisposedException
                    || e is SecurityException
                    || e is UnauthorizedAccessException)
                {
                    return e;
                }
                else
                {
                    throw;
                }
            }
            return null;
        }

        protected static Win32.RegistryKey GetHive(RegistryHive hive) => hive switch
        {
            RegistryHive.CurrentUser => Win32.Registry.CurrentUser,
            RegistryHive.LocalMachine => Win32.Registry.LocalMachine,
            RegistryHive.DefaultUser => Win32.Registry.Users.OpenSubKey(@".DEFAULT"),
            _ => throw new ArgumentOutOfRangeException(nameof(hive)),
        };

        /// <summary>
        ///  Returns the requested registry value. Will return default if unsuccessful.
        ///  Supports nullable types- returns null if unable to get requested value.
        /// </summary>
        /// <param name="hive">The root key to use.</param>
        /// <param name="subkeyName">Subkey name or subpath.</param>
        /// <param name="valueName">Name of value</param>
        public static T RetrieveRegistryValue<T>(RegistryHive hive, string subkeyName, string valueName)
        {
            var registrySubkey = OpenSubkey(GetHive(hive), subkeyName);

            if (registrySubkey is not null)
            {
                using (registrySubkey)
                {
                    return RetrieveRegistryValue<T>(registrySubkey, valueName);
                }
            }

            return default;
        }

        private static T RetrieveRegistryValue<T>(Win32.RegistryKey key, string valueName)
        {
            object registryValue = null;

            Exception exception = RegistryExceptionWrapper(() => registryValue = key.GetValue(valueName));
            if (exception is not null)
            {
                Debug.WriteLine("Unable to get value '{0}'.  Exception follows: \n{1}", valueName, exception);
            }

            if (registryValue is not null)
            {
                return Types.ConvertType<T>(registryValue);
            }

            return default;
        }

        private static Win32.RegistryKey OpenSubkey(Win32.RegistryKey registryKey, string subkeyName, bool writable = false)
        {
            Win32.RegistryKey registrySubkey = null;

            Exception exception = RegistryExceptionWrapper(() => registrySubkey = registryKey.OpenSubKey(subkeyName, writable));
            if (exception is not null)
            {
                Debug.WriteLine("Unable to open subkey '{0}'.  Exception follows: \n{1}", subkeyName, exception);
            }

            return registrySubkey;
        }

        private static IEnumerable<Win32.RegistryKey> GetSubkeys(Win32.RegistryKey registryKey)
        {
            foreach (string subkeyName in registryKey.GetSubKeyNames())
            {
                var subkey = OpenSubkey(registryKey, subkeyName);
                if (subkey is not null)
                {
                    yield return subkey;
                }
            }
        }

        /// <summary>
        ///  Gets the subkey names for the given key.
        /// </summary>
        /// <param name="hive">The root key to use.</param>
        /// <param name="subkeyName">Subkey name or subpath.</param>
        /// <returns>Subkey names if any.</returns>
        public static IEnumerable<string> GetSubkeyNames(RegistryHive hive, string subkeyName)
        {
            string[] subkeyNames = null;

            using (Win32.RegistryKey registryKey = OpenSubkey(GetHive(hive), subkeyName))
            {
                Exception exception = RegistryExceptionWrapper(() => subkeyNames = registryKey.GetSubKeyNames());
                if (exception is not null)
                {
                    Debug.WriteLine("Unable to get subkey names for key '{0}'.  Exception follows: \n{1}", registryKey.Name, exception);
                }
            }

            if (subkeyNames is not null)
            {
                return subkeyNames;
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        ///  Returns as <typeparamref name="T"/> all values of all subkeys of the specified key that are of type
        ///  <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   For example, if you have a key HKCU\Bla\Foo1 with values Name1="bar" and Name2="baz",
        ///   and HKCU\Bla\Foo2 with values "Name1"=1 (DWORD) and "" (default)="c:\users",
        ///   <see cref="RetrieveAllRegistrySubkeyValues{T}(RegistryHive, string)"/> for "Bla" would return
        ///   a three-element enumerable containing the strings "bar", "baz", and "c:\users".
        ///  </para>
        /// </remarks>
        /// <param name="hive">The root key to use.</param>
        /// <param name="subkeyName">Subkey name or subpath.</param>
        /// <returns>Array of values. Returns null on failure.</returns>
        public static IEnumerable<T> RetrieveAllRegistrySubkeyValues<T>(RegistryHive hive, string subkeyName)
        {
            var registrySubkey = OpenSubkey(GetHive(hive), subkeyName);

            if (registrySubkey is null)
            {
                return Enumerable.Empty<T>();
            }

            List<T> values = new();

            using (registrySubkey)
            {
                foreach (var subSubkey in GetSubkeys(registrySubkey))
                {
                    using (subSubkey)
                    {
                        string[] registryValueNames = null;
                        Exception exception = RegistryExceptionWrapper(() => registryValueNames = subSubkey.GetValueNames());
                        if (exception is not null)
                        {
                            Debug.WriteLine("Unable to get value names for key '{0}'.  Exception follows: \n{1}", subkeyName, exception);
                            continue;
                        }

                        foreach (string registryValueName in registryValueNames)
                        {
                            object rawValue = RetrieveRegistryValue<object>(subSubkey, registryValueName);

                            if (rawValue is not null)
                            {
                                T value = Types.ConvertType<T>(rawValue);
                                values.Add(value);
                            }
                        }
                    }
                }
            }

            return values;
        }

        /// <summary>
        ///  Sets or deletes the desired registry value using the designated registry value kind.
        ///  Will create the specified subkey if it does not exist.
        /// </summary>
        /// <remarks>
        /// This will use the following value types:
        ///  int => DWORD
        ///  byte[] => Binary
        ///  string[] => MultiString
        ///  other array => FAILS
        ///  other => String
        /// </remarks>
        /// <param name="hive">The root key to use.</param>
        /// <param name="subkeyName">Subkey name or subpath.</param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">The value to set or null to delete the existing value, if any.</param>
        /// <param name="valueKind">Kind of registry value</param>
        /// <returns>'true' if successful.</returns>
        public static bool SetRegistryValue<T>(RegistryHive hive, string subkeyName, string valueName, T value)
        {
            // Causes the behavior detailed in the remarks above
            Win32.RegistryValueKind valueKind = Win32.RegistryValueKind.Unknown;
            Win32.RegistryKey registryKey = GetHive(hive);

            var registrySubkey = OpenSubkey(registryKey, subkeyName, writable: true);

            if (registrySubkey is null)
            {
                // Key not found, now create it
                if (value is null)
                {
                    // No point in creating a subkey for a value we're going to delete
                    return true;
                }

                Exception exception = RegistryExceptionWrapper(() => registrySubkey = registryKey.CreateSubKey(subkeyName));
                if (exception is not null)
                {
                    Debug.WriteLine("Unable to create sub key '{0}'.  Exception follows: \n{1}", subkeyName, exception);
                    return false;
                }
            }

            if (registrySubkey is not null)
            {
                try
                {
                    Exception exception = null;

                    if (value is null)
                    {
                        // Value specified is null- delete
                        exception = RegistryExceptionWrapper(() => registrySubkey.DeleteValue(valueName, false));
                    }
                    else
                    {
                        // Normal set
                        exception = RegistryExceptionWrapper(() => registrySubkey.SetValue(valueName, value, valueKind));
                    }

                    if (exception is not null)
                    {
                        Debug.WriteLine("Unable to set value '{0}'.  Exception follows: \n{1}", valueName, exception);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                finally
                {
                    registrySubkey.Close();
                }
            }

            return false;
        }
    }
}
