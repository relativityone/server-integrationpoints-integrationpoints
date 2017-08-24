using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Helpers
{
	public class NonValidCharactersValidator : INonValidCharactersValidator
	{
		public ValidationResult Validate(string name, string errorMessage)
		{
			var result = new ValidationResult();

			if (!ValidateSpecialCharactersOccurences(name))
			{
				result.Add(errorMessage);
			}

			return result;
		}

		private static bool ValidateSpecialCharactersOccurences(string text)
		{
			var pattern = "^[^<>:\\\"\\\\\\/|\\?\\*]*$";
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			Match match = regex.Match(text);

			//If validated string doesn't contain any illegal characters
			return match.Success;
		}
	}
}
