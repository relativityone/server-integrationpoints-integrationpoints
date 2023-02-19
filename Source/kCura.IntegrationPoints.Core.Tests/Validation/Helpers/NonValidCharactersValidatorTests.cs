using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.Helpers
{
    [TestFixture, Category("Unit")]
    public class NonValidCharactersValidatorTests
    {
        private const char _FIRST_NON_PRINTABLE_ASCII_CHARACTER = (char)0;
        private const char _LAST_NON_PRINTABLE_ASCII_CHARACTER = (char)31;
        private const char _FIRST_PRINTABLE_ASCII_CHARACTER = (char)32;
        private const char _LAST_PRINTABLE_ASCII_CHARACTER = (char)126;
        private const string _ERROR_MESSAGE = "EM";
        private readonly char[] _forbiddenPrintableCharacters = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        private readonly NonValidCharactersValidator _validator = new NonValidCharactersValidator();

        [TestCase("abc")]
        [TestCase("Name^")]
        [TestCase("Name with spaces")]
        public void ItShouldReturnValidResultForValidData(string name)
        {
            ValidationResult validationResult = _validator.Validate(name, _ERROR_MESSAGE);

            Assert.IsTrue(validationResult.IsValid);
            Assert.IsEmpty(validationResult.MessageTexts);
        }

        [Test]
        public void ItShouldReturnValidResultForAsciiCharactersExceptForbidden()
        {
            foreach (char asciiCharacter in GetAllowedAsciiCharacters())
            {
                string name = $"IntegrationPoint{asciiCharacter}";
                ValidationResult validationResult = _validator.Validate(name, _ERROR_MESSAGE);

                Assert.IsTrue(validationResult.IsValid);
            }
        }

        [Test]
        public void ItShouldReturnInvalidResultForForbiddenCharactersPresent()
        {
            foreach (char character in _forbiddenPrintableCharacters)
            {
                string name = $"IntegrationPoint{character}";
                ValidationResult validationResult = _validator.Validate(name, _ERROR_MESSAGE);

                Assert.IsFalse(validationResult.IsValid);
            }
        }

        [Test]
        public void ItShouldReturnInvalidResultForTabPresentInName()
        {
            const string name = "a\tb";
            ValidationResult validationResult = _validator.Validate(name, _ERROR_MESSAGE);

            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ItShouldReturnInvalidResultForAsciiCodesFrom0To31PresentInName()
        {
            for (char c = _FIRST_NON_PRINTABLE_ASCII_CHARACTER; c <= _LAST_NON_PRINTABLE_ASCII_CHARACTER; c++)
            {
                string invalidName = $"Integration Point {c}";
                ValidationResult validationResult = _validator.Validate(invalidName, _ERROR_MESSAGE);

                Assert.IsFalse(validationResult.IsValid);
            }
        }

        [TestCase("Integration point is invalid")]
        [TestCase("Invalid IP name !!!")]
        public void ItShouldSetProperErrorMessage(string errorMessage)
        {
            const string name = "InvalidName\\";
            ValidationResult validationResult = _validator.Validate(name, errorMessage);

            Assert.AreEqual(1, validationResult.MessageTexts.Count());
            string actualMessage = validationResult.MessageTexts.First();
            Assert.AreEqual(errorMessage, actualMessage);
        }

        private IEnumerable<char> GetAllowedAsciiCharacters()
        {
            for (char asciiCharacter = _FIRST_PRINTABLE_ASCII_CHARACTER; asciiCharacter <= _LAST_PRINTABLE_ASCII_CHARACTER; asciiCharacter++)
            {
                if (!_forbiddenPrintableCharacters.Contains(asciiCharacter))
                {
                    yield return asciiCharacter;
                }
            }
        }
    }
}
