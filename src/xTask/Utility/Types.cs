// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace XTask.Utility
{
    public static class Types
    {
        /// <summary>
        /// Converts the source object to the desired type if possible.  Returns default of T if it cannot.  Nullable tolerant.
        /// </summary>
        /// <remarks>
        /// This method tries to be as flexible as possible, allowing conversion to nullable for source primitives (including enums).
        /// This allows you to ask for nullable of int for an underlying int source object- this way you can know
        /// the conversion failed if you care to.  It also also allows you to ask for the primitive type for a source
        /// nullable type if you don't care about conversion success.
        /// </remarks>
        public static T ConvertType<T>(object source)
        {
            if (source is null) { return default; }

            Type type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // For nullable types we want to use the underlying type for the conversion
                type = type.GetGenericArguments()[0];
            }

            if (type.IsAssignableFrom(source.GetType()))
            {
                // Assignable- cast and return
                return (T)source;
            }

            if (type.IsPrimitive)
            {
                // Primitive type, use Convert
                try
                {
                    return (T)Convert.ChangeType(source, type, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    if (e is InvalidCastException
                        || e is ArgumentNullException
                        || e is FormatException
                        || e is OverflowException)
                    {
                        // One special case here- conceptually converting a number to a bool makes a lot of sense as "0" == false for *all* primitive types, but
                        // converting a string or char representation of a number will fail (only "true" and "false" are valid).
                        // If we have a source object that can be converted to double, use that for the conversion (could potentially use decimal, but
                        // double seems to be more likely to have a valid converter)
                        if (type == typeof(bool))
                        {
                            double? numericBool = ConvertType<double?>(source);
                            if (numericBool.HasValue)
                            {
                                return (T)Convert.ChangeType(numericBool.Value, type, CultureInfo.InvariantCulture);
                            }
                        }

                        ConversionFailed(source, e);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                // Not a primitive type that was requested- give the default TypeConverter a try
                try
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
                    if (typeConverter is not null && typeConverter.CanConvertFrom(source.GetType()))
                    {
                        return (T)typeConverter.ConvertFrom(source);
                    }
                    else
                    {
                        if (type.IsEnum && source is not null)
                        {
                            try
                            {
                                T convertedEnum = (T)Enum.Parse(type, source.ToString(), ignoreCase: true);
                                if (Enum.IsDefined(type, convertedEnum))
                                {
                                    return convertedEnum;
                                }
                                else
                                {
                                    if (default(T) is null)
                                    {
                                        // We were asked for a nullable enum, allow a null
                                        return default;
                                    }

                                    Array values = Enum.GetValues(type);
                                    if (values.Length > 0)
                                    {
                                        return (T)values.GetValue(0);
                                    }

                                    return default;
                                }
                            }
                            catch (Exception e)
                            {
                                if (e is InvalidCastException
                                    || e is ArgumentNullException
                                    || e is FormatException
                                    || e is OverflowException)
                                {
                                    ConversionFailed(source, e);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "No converter found for type '{0}'", type));
                    }
                }
                catch (Exception e)
                {
                    if (e is InvalidCastException
                        || e is ArgumentNullException
                        || e is FormatException
                        || e is OverflowException
                        || e is NotSupportedException)
                    {
                        ConversionFailed(source, e);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Did we ask for a string?  Try it if the TypeConverter failed first.
            if (type == typeof(string))
            {
                return (T)(object)source.ToString();
            }

            return default;
        }

        /// <summary>
        /// Gets the attributes of the specified type, if any
        /// </summary>
        /// <param name="inherit">Walks the inheritance chain to look for attributes</param>
        public static IEnumerable<T> GetAttributes<T>(this object target, bool inherit = false) where T : Attribute
        {
            Type targetType = target.GetType();
            foreach (object attribute in targetType.GetCustomAttributes(typeof(T), inherit))
            {
                yield return (T)attribute;
            }
        }

        [Conditional("DEBUG")]
        private static void ConversionFailed(object source, Exception e)
        {
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to convert value '{0}'.  Exception follows: \n{1}", source, e));
        }
    }
}