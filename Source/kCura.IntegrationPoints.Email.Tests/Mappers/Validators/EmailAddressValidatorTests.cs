using FluentAssertions;
using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Mappers.Validators;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Email.Tests.Mappers.Validators
{
    [TestFixture, Category("Unit")]
    public class EmailAddressValidatorTests
    {
        [TestCase("relativity.admin@kcura.com")]
        [TestCase("1234567890@domain.com")]
        [TestCase("email@doma-in.com")]
        [TestCase("email@123.123.123.123")]
        [TestCase("firstname-lastname@domain.com")]
        public void ShouldReturnSuccessForValidEmail(string validEmail)
        {
            // act
            LanguageExt.Validation<string, ValidEmailAddress> validation = EmailAddressValidator.ValidateEmailAddress(validEmail);

            // assert
            validation.IsSuccess.Should().BeTrue("because email address was valid");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("          ")]
        public void ShouldFailForEmptyEmail(string emptyEmail)
        {
            // act
            LanguageExt.Validation<string, ValidEmailAddress> validation = EmailAddressValidator.ValidateEmailAddress(emptyEmail);

            // assert
            validation.IsFail.Should().BeTrue("because email address was empty");
            validation.FailAsEnumerable().Should().Contain("Email address cannot be empty");
        }

        [TestCase("plainaddress")]
        [TestCase("@domain.com")]
        public void ShouldFailForInvalidEmail(string invalidEmail)
        {
            // act
            LanguageExt.Validation<string, ValidEmailAddress> validation = EmailAddressValidator.ValidateEmailAddress(invalidEmail);

            // assert
            validation.IsFail.Should().BeTrue("because email address was invalid");
            validation.FailAsEnumerable().Should().Contain("Email address is invalid");
        }
    }
}
