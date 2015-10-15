using System;
using System.Globalization;

namespace Qube.Extensions
{
    public static class StringExtensions
    {
        public static string ToStringSafe(this object value, string format = "{0}")
        {
            return value.ToStringSafe("{0}", null);
        }
        public static string ToStringSafe(this object value, string format = "{0}", CultureInfo ci = null)
        {
            return value == null ? string.Empty : String.Format(ci, format, value);
        }
    }
}