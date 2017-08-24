using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.Helpers
{
	[TestFixture]
	public class NonValidCharactersValidatorTests
	{
		private string _errorMessage = "EM";
		private readonly char[] _forbiddenCharacters = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
		private readonly NonValidCharactersValidator _validator = new NonValidCharactersValidator();

		[TestCase("abc")]
		[TestCase("Name^")]
		[TestCase("Name with spaces")]
		public void ItShouldReturnValidResultForValidData(string name)
		{
			ValidationResult validationResult = _validator.Validate(name, _errorMessage);

			Assert.IsTrue(validationResult.IsValid);
			Assert.IsEmpty(validationResult.Messages);
		}

		[Test]
		public void ItShouldReturnValidResultForASCIICharactersExceptForbidden()
		{
			foreach (char asciiCharacter in GetAllowedAsciiCharacters())
			{
				string name = $"IntegrationPoint{asciiCharacter}";
				ValidationResult validationResult = _validator.Validate(name, _errorMessage);

				Assert.IsTrue(validationResult.IsValid);
			}
		}

		[Test]
		public void ItShouldReturnInvalidResultForForbiddenCharactersPresent()
		{
			foreach (char character in _forbiddenCharacters)
			{
				string name = $"IntegrationPoint{character}";
				ValidationResult validationResult = _validator.Validate(name, _errorMessage);

				Assert.IsFalse(validationResult.IsValid);
			}
		}

		[TestCase("Integration point is invalid")]
		[TestCase("Invalid IP name !!!")]
		public void ItShouldSetProperErrorMessage(string errorMessage)
		{
			string name = "InvalidName\\";
			ValidationResult validationResult = _validator.Validate(name, errorMessage);

			Assert.AreEqual(1, validationResult.Messages.Count());
			var actualMessage = validationResult.Messages.First();
			Assert.AreEqual(errorMessage, actualMessage);
		}

		private IEnumerable<char> GetAllowedAsciiCharacters()
		{
			var firstPrintableASCIICharacter = (char)32;
			var lastPrintableASCIICharacter = (char)126;
			for (char asciiCharacter = firstPrintableASCIICharacter; asciiCharacter <= lastPrintableASCIICharacter; asciiCharacter++)
			{
				if (!_forbiddenCharacters.Contains(asciiCharacter))
				{
					yield return asciiCharacter;
				}
			}
		}
	}
}
