using System;
using System.Linq;

namespace Relativity.Sync.Utils
{
    internal static class StringExtensions
    {
        internal static string LimitLength(string value)
        {
            const int maxLength = 50;
            const int two = 2;
            const int three = 3;
            const string truncationEnding = "...";

            string truncatedValue = value.Length > maxLength
                ? value.Remove((maxLength / two) - three) + truncationEnding + value.Substring(value.Length - (maxLength / two))
                : value;

            return truncatedValue;
        }

        public static bool IsIn(this string value, StringComparison stringComparison, params string[] inclusions)
        {
            return inclusions.Any(x => string.Equals(x, value, stringComparison));
        }
    }
}
