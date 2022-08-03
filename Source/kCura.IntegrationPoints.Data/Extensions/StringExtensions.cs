using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Data.Extensions
{
    internal static class StringExtensions
    {
        public static string EscapeSingleQuote(this string s)
        {
            return Regex.Replace(s, "'", "\\'");
        }
    }
}
