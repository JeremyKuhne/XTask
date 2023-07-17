// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace XTask.Collections
{
    /// <summary>
    ///  Useful extensions for IDictionary(TKey,TValue).
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Updates the key/value pair if the key already exists.
        /// </summary>
        /// <returns>"true" if updated</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
        public static bool UpdateIfPresent<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (source.ContainsKey(key))
            {
                source[key] = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Adds a key/value pair if the key does not already exist, or updates the key/value pair
        ///  if the key already exists.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (source.ContainsKey(key))
            {
                source[key] = value;
            }
            else
            {
                source.Add(key, value);
            }
        }

        /// <summary>
        ///  Adds a key/value pair if the key does not already exist, or updates the key/value pair
        ///  if the key already exists.
        /// </summary>
        /// <param name="addValue">Value to use if adding.</param>
        /// <param name="updateValueFactory">Value factory to use if updating (takes originalKey & originalValue, returns updatedValue).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or <paramref name="updateValueFactory"/> are null.</exception>
        /// <returns>The new value for the key.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source, TKey key,
            TValue addValue,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (updateValueFactory is null) { throw new ArgumentNullException(nameof(updateValueFactory)); }
            if (source.ContainsKey(key))
            {
                TValue updatedValue = updateValueFactory(key, source[key]);
                source[key] = updatedValue;
                return updatedValue;
            }
            else
            {
                source.Add(key, addValue);
                return addValue;
            }
        }

        /// <summary>
        ///  Adds a key/value pair if the key does not already exist, or updates the key/value pair
        ///  if the key already exists.
        /// </summary>
        /// <param name="addValue">Value to use if adding.</param>
        /// <param name="updateValueFactory">Value factory to use if updating (takes originalValue, returns updatedValue).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or <paramref name="updateValueFactory"/> are null.</exception>
        /// <returns>The new value for the key.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source, TKey key,
            TValue addValue,
            Func<TValue, TValue> updateValueFactory)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (updateValueFactory is null) { throw new ArgumentNullException(nameof(updateValueFactory)); }
            if (source.ContainsKey(key))
            {
                TValue updatedValue = updateValueFactory(source[key]);
                source[key] = updatedValue;
                return updatedValue;
            }
            else
            {
                source.Add(key, addValue);
                return addValue;
            }
        }

        /// <summary>
        ///  Adds a key/value pair if the key does not already exist, or updates the key/value pair
        ///  if the key already exists.
        /// </summary>
        /// <param name="addValueFactory">Value factory to use if adding (takes originalKey and returns updatedValue).</param>
        /// <param name="updateValueFactory">Value factory to use if updating (takes originalKey & originalValue, returns updatedValue).</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>, <paramref name="addValueFactory"/>, or <paramref name="updateValueFactory"/> is null.</exception>
        /// <returns>The new value for the key.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (addValueFactory is null) { throw new ArgumentNullException(nameof(addValueFactory)); }
            if (updateValueFactory is null) { throw new ArgumentNullException(nameof(updateValueFactory)); }
            if (source.ContainsKey(key))
            {
                TValue updatedValue = updateValueFactory(key, source[key]);
                source[key] = updatedValue;
                return updatedValue;
            }
            else
            {
                TValue adddedValue = addValueFactory(key);
                source.Add(key, adddedValue);
                return adddedValue;
            }
        }

        /// <summary>
        ///  Adds a key/value pair if the key does not already exist.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
        /// <returns>The value for the key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            TValue value)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (source.ContainsKey(key))
            {
                return source[key];
            }
            else
            {
                source.Add(key, value);
                return value;
            }
        }

        /// <summary>
        ///  Adds a key/value pair if the key does not already exist.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or <paramref name="addValueFactory"/> are null.</exception>
        /// <returns>The value for the key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            Func<TKey, TValue> addValueFactory)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            if (addValueFactory is null) { throw new ArgumentNullException(nameof(addValueFactory)); }
            if (!source.TryGetValue(key, out TValue value))
            {
                value = addValueFactory(key);
                source.Add(key, value);
            }
            return value;
        }

        /// <summary>
        ///  Removes all entries with the specified value.
        /// </summary>
        /// <returns>"true" if anything was removed</returns>
        public static bool TryRemoveValues<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TValue value)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }

            List<TKey> keysForValue = new();
            foreach (KeyValuePair<TKey, TValue> entry in source)
            {
                if (entry.Value.Equals(value))
                {
                    keysForValue.Add(entry.Key);
                }
            }
            return source.TryRemove(keysForValue.ToArray());
        }

        /// <summary>
        ///  Attempts to a set of keys from the dictionary.
        /// </summary>
        /// <returns>'true' if any successfully removed</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="keys"/> is null.</exception>
        public static bool TryRemove<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            params TKey[] keys)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (keys is null) { throw new ArgumentNullException(nameof(keys)); }

            bool anyRemoved = false;
            foreach (TKey key in keys)
            {
                anyRemoved |= source.TryRemove(key);
            }
            return anyRemoved;
        }

        /// <summary>
        ///  Attempts to remove a key from the dictionary.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
        /// <returns>'true' if successfully removed</returns>
        public static bool TryRemove<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }
            return source.Remove(key);
        }

        /// <summary>
        ///  Attempts to remove key and return the value from the dictionary.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
        /// <returns>'true' if successfully removed</returns>
        public static bool TryRemove<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            out TValue value)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            if (source.TryGetValue(key, out value))
            {
                return source.Remove(key);
            }
            return false;
        }
    }
}