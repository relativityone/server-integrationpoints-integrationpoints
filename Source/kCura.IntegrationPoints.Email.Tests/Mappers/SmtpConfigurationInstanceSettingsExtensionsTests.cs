using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Mappers;
using LanguageExt;
using NUnit.Framework;
using FluentAssertions;

namespace kCura.IntegrationPoints.Email.Tests.Mappers
{
    [TestFixture, Category("Unit")]
    public class SmtpConfigurationInstanceSettingsExtensionsTests
    {
        [Test]
        public void ShouldReturnsSuccessForValidInput()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: 3,
                useSsl: true,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> x = sut.ConvertToSmtpClientSettings();

            // assert
            x.IsSuccess.Should().BeTrue("because configuration was valid");
        }

        [Test]
        public void ShouldReturnErrorWhenPortIsEmpty()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: null,
                useSsl: true,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> clientSettingsValidation = sut.ConvertToSmtpClientSettings();

            // assert
            clientSettingsValidation.IsSuccess.Should().BeFalse("because configuration was invalid");
            Seq<string> errors = clientSettingsValidation.FailAsEnumerable();
            errors.Count.Should().Be(1, "because configuration has one error");
            errors.Should().Contain("The SMTP port was not specified.");
        }

        [Test]
        public void ShouldReturnErrorWhenPortIsTooBig()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: 65536,
                useSsl: true,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> clientSettingsValidation = sut.ConvertToSmtpClientSettings();

            // assert
            clientSettingsValidation.IsSuccess.Should().BeFalse("because configuration was invalid");
            Seq<string> errors = clientSettingsValidation.FailAsEnumerable();
            errors.Count.Should().Be(1, "because configuration has one error");
            errors.Should().Contain("Maximum allowed value for port number is: 65535.");
        }

        [Test]
        public void ShouldReturnErrorWhenDomainIsInvalid()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "",
                port: 43,
                useSsl: true,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> clientSettingsValidation = sut.ConvertToSmtpClientSettings();

            // assert
            clientSettingsValidation.IsSuccess.Should().BeFalse("because configuration was invalid");
            Seq<string> errors = clientSettingsValidation.FailAsEnumerable();
            errors.Count.Should().Be(1, "because configuration has one error");
            errors.Should().Contain("The SMTP host was not specified.");
        }

        [Test]
        public void ShouldReturnErrorWhenUseSslValueIsMissing()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: 32,
                useSsl: null,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> clientSettingsValidation = sut.ConvertToSmtpClientSettings();

            // assert
            clientSettingsValidation.IsSuccess.Should().BeFalse("because configuration was invalid");
            Seq<string> errors = clientSettingsValidation.FailAsEnumerable();
            errors.Count.Should().Be(1, "because configuration has one error");
            errors.Should().Contain("UseSsl SMTP config value was not specified.");
        }

        [Test]
        public void ShouldReturnErrorWhenEmailIsInvalid()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: 2,
                useSsl: true,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> clientSettingsValidation = sut.ConvertToSmtpClientSettings();

            // assert
            clientSettingsValidation.IsSuccess.Should().BeFalse("because configuration was invalid");
            Seq<string> errors = clientSettingsValidation.FailAsEnumerable();
            errors.Count.Should().Be(1, "because configuration has one error");
            errors.Should().Contain("Email address is invalid");
        }

        [Test]
        public void ShouldReturnErrorsWhenPortAndDomainAreInvalid()
        {
            // arrange
            var sut = new SmtpConfigurationDto(
                domain: "",
                port: null,
                useSsl: true,
                userName: "admin",
                password: "1234",
                emailFromAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, ValidSmtpConfigurationDto> clientSettingsValidation = sut.ConvertToSmtpClientSettings();

            // assert
            clientSettingsValidation.IsFail.Should().BeTrue("because port and domain was invalid");
            Seq<string> errors = clientSettingsValidation.FailAsEnumerable();
            errors.Count().Should().Be(2, "because in configuration were 2 errors");
            errors.Should().Contain("The SMTP port was not specified.");
            errors.Should().Contain("The SMTP host was not specified.");
        }
    }
}
