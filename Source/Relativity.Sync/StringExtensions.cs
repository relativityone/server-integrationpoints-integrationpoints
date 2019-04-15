namespace Relativity.Sync
{
	internal static class StringExtensions
	{
		internal static string Truncate(string value, int maxLength)
		{
			const int two = 2;
			const int three = 3;
			const string truncationEnding = "...";

			string truncatedValue = value.Length > maxLength
				? value.Remove(maxLength / two - three) + truncationEnding + value.Substring(value.Length - maxLength / two)
				: value;

			return truncatedValue;
		}
	}
}