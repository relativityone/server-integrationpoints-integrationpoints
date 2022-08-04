using System.Linq;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class NameValidatorTests
    {
        [Test]
        public void ItShouldValidateProperName()
        {
            // arrange
            var name = "the name";

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validationResult = new ValidationResult();
            nonValidCharactersValidator.Validate(Arg.Any<string>(), Arg.Any<string>()).Returns(validationResult);
            var validator = new NameValidator(nonValidCharactersValidator);

            // act
            var actual = validator.Validate(name);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
        }

        [TestCase(null)]
        [TestCase("\t")]
        [TestCase("    ")]
        [TestCase("\r\n")]
        public void ItShouldFailValidationForEmptyOrWhitespaceName(string name)
        {
            // arrange
            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validationResult = new ValidationResult();
            nonValidCharactersValidator.Validate(Arg.Any<string>(), Arg.Any<string>()).Returns(validationResult);
            var validator = new NameValidator(nonValidCharactersValidator);

            // act
            var actual = validator.Validate(name);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Contains(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_NAME_EMPTY));
        }

        [TestCase("first:second")]
        [TestCase("pi | pe")]
        public void ItShouldCallNonValidCharactersValidatorWithProperArguments(string name)
        {
            // arrange
            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new NameValidator(nonValidCharactersValidator);

            // act
            validator.Validate(name);

            // assert
            nonValidCharactersValidator.Received().Validate(name,
                IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_NAME_CONTAINS_ILLEGAL_CHARACTERS);
        }

        [TestCase(false, "EM1")]
        [TestCase(false, "Error message")]
        [TestCase(true, null)]
        [TestCase(false, null)]
        public void ItShouldReturErrorsReturnedByNonValidCharactersValidatorForNonEmptyName(bool isValid, string errorMessage)
        {
            // arrange
            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            ValidationResult validationResult = errorMessage != null
                ? new ValidationResult(isValid, errorMessage)
                : new ValidationResult(isValid);
            nonValidCharactersValidator.Validate(Arg.Any<string>(), Arg.Any<string>()).Returns(validationResult);
            var validator = new NameValidator(nonValidCharactersValidator);

            // act
            ValidationResult actual = validator.Validate("IpName");

            // assert
            Assert.AreEqual(isValid, actual.IsValid);
            if (errorMessage != null)
            {
                Assert.IsTrue(actual.MessageTexts.Contains(errorMessage));
            }
        }

        [Test]
        public void ItShouldReturnValidKey()
        {
            // arrange
            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new NameValidator(nonValidCharactersValidator);

            // act
            string actual = validator.Key;

            // assert
            Assert.AreEqual(Constants.IntegrationPointProfiles.Validation.NAME, actual);
        }
    }
}