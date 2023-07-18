// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace XTask.Utility
{
    public static class Enums
    {
        /// <summary>
        ///  Returns an array of set values in the given enum
        /// </summary>
        /// <remarks>
        ///  This will return each set value if you have [Flags], or the single value if you don't
        /// </remarks>
        public static Array GetSetValues(Enum enumeration)
        {
            List<object> setValues = new();
            foreach (object possibleValue in Enum.GetValues(enumeration.GetType()))
            {
                if (enumeration.HasFlag((Enum)possibleValue))
                {
                    setValues.Add(possibleValue);
                }
            }

            return setValues.ToArray();
        }

        // This works, but is less than ideal. You can't constrain to Enum unfortunately. (where T : Enum)
        //public static IEnumerable<T> GetSetValues<T>(T enumeration)
        //{
        //  Type enumerationType = typeof(T);
        //  if (!enumerationType.IsSubclassOf(typeof(Enum)))
        //  {
        //    throw new ArgumentException("Argument must be an enum.", "enumeration");
        //  }

        //  Enum castEnum = (Enum)(object)enumeration;

        //  List<T> setValues = new List<T>();
        //  foreach (object possibleValue in Enum.GetValues(typeof(T)))
        //  {
        //    if (castEnum.HasFlag((Enum)possibleValue))
        //    {
        //      setValues.Add((T)possibleValue);
        //    }
        //  }

        //  return setValues;
        //}
    }
}