using System.Text.RegularExpressions;

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
				? value.Remove(maxLength / two - three) + truncationEnding + value.Substring(value.Length - maxLength / two)
				: value;

			return truncatedValue;
		}

		/// <summary>
		/// Replaces a group from regex in a string
		/// </summary>
		/// <param name="text">Text</param>
		/// <param name="regex">Regex with groups</param>
		/// <param name="groupToReplace">0-based index of a group</param>
		/// <param name="replaceWith">text to replace with</param>
		/// <returns></returns>
		internal static string ReplaceGroup(this string text, string regex, int groupToReplace, string replaceWith)
		{
			groupToReplace++; // Match.Groups collection indexes start at 1, but 0-based is more natural in C#
			Match matches = Regex.Match(text, regex, RegexOptions.IgnoreCase);
			if (matches.Success 
			    && matches.Groups.Count >= groupToReplace 
			    && matches.Groups[groupToReplace].Success)
			{
				return text.Replace(matches.Groups[groupToReplace].Value, replaceWith);
			}

			return text;
		}
	}
}