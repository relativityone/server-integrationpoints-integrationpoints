namespace Relativity.Sync
{
    internal static class KeplerQueryHelpers
    {
        /// <summary>
        /// Escapes characters that may cause parsing errors when surrounded by single quotes.
        /// </summary>
        /// <returns>New string with relevant characters escaped with a backslash.</returns>
        public static string EscapeForSingleQuotes(string value)
        {
            return value
                .Replace(@"\", @"\\")
                .Replace(@"'", @"\'");
        }
    }
}
