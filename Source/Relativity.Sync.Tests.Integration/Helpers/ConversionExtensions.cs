using System;
using System.Globalization;

namespace Relativity.Sync.Tests.Integration.Helpers
{
    internal static class ConversionExtensions
    {
        public static object ConvertTo<T>(this object value)
        {
            return ConvertTo(value, typeof(T));
        }

        public static object ConvertTo(this object value, Type target)
        {
            if (target == typeof(int))
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }

            if (target == typeof(long))
            {
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }

            if (target == typeof(bool))
            {
                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }

            if (target == typeof(string))
            {
                return value is DBNull ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            throw new ArgumentException($"Method does not know how to convert to type {target}");
        }
    }
}
