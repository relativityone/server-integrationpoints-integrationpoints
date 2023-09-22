using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Data.Extensions
{
    internal static class StringExtensions
    {
        public static string EscapeSingleQuote(this string s)
        {
            return Regex.Replace(s, "'", "\\'");
        }

        public static bool IsIn(this string value, StringComparison stringComparison, params string[] inclusions)
        {
            return inclusions.Any(x => string.Equals(x, value, stringComparison));
        }
    }
}
