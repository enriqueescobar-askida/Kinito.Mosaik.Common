// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringBuilderExtensions.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StringBuilderExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Alphamosaik.Common.Library
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder SetNewString(this StringBuilder source, string value)
        {
            source.Clear();
            source.Append(value);
            return source;
        }

        public static StringBuilder Clear(this StringBuilder source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.Length = 0;

            return source;
        }

        public static bool Contains(this StringBuilder source, string value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        public static bool IsNullOrEmpty(StringBuilder value)
        {
            if (value != null)
            {
                return value.Length == 0;
            }

            return true;
        }

        public static int IndexOf(this StringBuilder source, string value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(stringValue, value);
        }

        public static int IndexOf(this StringBuilder source, string value, int startIndex)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(stringValue, value, startIndex);
        }

        public static int IndexOf(this StringBuilder source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, 0, source.Length, comparisonType);
        }

        public static int IndexOf(this StringBuilder source, string value, int startIndex, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if ((count < 0) || (count > (source.Length - startIndex)))
            {
                throw new ArgumentOutOfRangeException("count");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.None);
        }

        public static int IndexOf(this StringBuilder source, string value, int startIndex, StringComparison comparisonType)
        {
            return source.IndexOf(value, startIndex, source.Length - startIndex, comparisonType);
        }

        public static int IndexOf(this StringBuilder source, string value, int startIndex, int count, StringComparison comparisonType)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if ((count < 0) || (startIndex > (source.Length - count)))
            {
                throw new ArgumentOutOfRangeException("count");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.Ordinal);

                case StringComparison.OrdinalIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(stringValue, value, startIndex, count, CompareOptions.OrdinalIgnoreCase);
            }

            throw new ArgumentException("Not Supported StringComparison");
        }

        public static int LastIndexOf(this StringBuilder source, string value)
        {
            return source.LastIndexOf(value, source.Length - 1, source.Length, StringComparison.CurrentCulture);
        }

        public static int LastIndexOf(this StringBuilder source, string value, int startIndex)
        {
            return source.LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
        }

        public static int LastIndexOf(this StringBuilder source, string value, StringComparison comparisonType)
        {
            return source.LastIndexOf(value, source.Length - 1, source.Length, comparisonType);
        }

        public static int LastIndexOf(this StringBuilder source, string value, int startIndex, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.None);
        }

        public static int LastIndexOf(this StringBuilder source, string value, int startIndex, StringComparison comparisonType)
        {
            return source.LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
        }

        public static int LastIndexOf(this StringBuilder source, string value, int startIndex, int count, StringComparison comparisonType)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if ((source.Length == 0) && ((startIndex == -1) || (startIndex == 0)))
            {
                if (value.Length != 0)
                {
                    return -1;
                }

                return 0;
            }

            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if (startIndex == source.Length)
            {
                startIndex--;
                if (count > 0)
                {
                    count--;
                }

                if (((value.Length == 0) && (count >= 0)) && (((startIndex - count) + 1) >= 0))
                {
                    return startIndex;
                }
            }

            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException("count");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.Ordinal);

                case StringComparison.OrdinalIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(stringValue, value, startIndex, count, CompareOptions.OrdinalIgnoreCase);
            }

            throw new ArgumentException("Not Supported StringComparison");
        }

        public static string Substring(this StringBuilder source, int startIndex)
        {
            return source.Substring(startIndex, source.Length - startIndex);
        }

        public static string Substring(this StringBuilder source, int startIndex, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return stringValue.Substring(startIndex, length);
        }

        public static string ToLower(this StringBuilder source)
        {
            return source.ToLower(CultureInfo.CurrentCulture);
        }

        public static string ToLower(this StringBuilder source, CultureInfo culture)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return culture.TextInfo.ToLower(stringValue);
        }

        public static string ToLowerInvariant(this StringBuilder source)
        {
            return source.ToLower(CultureInfo.InvariantCulture);
        }

        public static string ToUpper(this StringBuilder source)
        {
            return source.ToUpper(CultureInfo.CurrentCulture);
        }

        public static string ToUpper(this StringBuilder source, CultureInfo culture)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }

// ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

// ReSharper restore PossibleNullReferenceException
            return culture.TextInfo.ToUpper(stringValue);
        }

        public static string ToUpperInvariant(this StringBuilder source)
        {
            return source.ToUpper(CultureInfo.InvariantCulture);
        }

        public static bool StartsWith(this StringBuilder source, string value)
        {
            return source.StartsWith(value, false, null);
        }

        public static bool StartsWith(this StringBuilder source, string value, bool ignoreCase, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // ReSharper disable PossibleNullReferenceException
            var stringValue = (string)source.GetType().GetField("m_StringValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);

            // ReSharper restore PossibleNullReferenceException
            if (stringValue == value)
            {
                return true;
            }

            CultureInfo info = culture ?? CultureInfo.CurrentCulture;
            return info.CompareInfo.IsPrefix(stringValue, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }
    }
}
