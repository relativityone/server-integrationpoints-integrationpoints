using System.Linq;
using System.Net.Mail;
using FluentAssertions;
using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Mappers;
using LanguageExt;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Email.Tests.Mappers
{
    [TestFixture, Category("Unit")]
    public class EmailMessageDtoExtensionsTests
    {
        private readonly ValidEmailAddress validEmailAdress = new ValidEmailAddress("relativity.admin@kcura.com");

        [Test]
        public void ShouldReturnSuccessForValidInput()
        {
            // arrange
            var emailMessageDto = new EmailMessageDto(
                subject: "subject",
                body: "body",
                toAddress: "relativity.admin@kcura.com"
            );

            // act
            Validation<string, MailMessage> mailMessageValidation = emailMessageDto.ConvertToMailMessage(fromAddress: validEmailAdress);

            // assert
            mailMessageValidation.Match(
                Succ: mailMessage => ValidateMailMessage(mailMessage, emailMessageDto), 
                Fail: errors => Assert.Fail("Should return success because input was valid")
            );
        }

        [Test]
        public void ShouldReturnFailureForInvalidToAddress()
        {
            // arrange
            var emailMessageDto = new EmailMessageDto(
                subject: "subject",
                body: "body",
                toAddress: "invalid"
            );

            // act
            Validation<string, MailMessage> mailMessageValidation = emailMessageDto.ConvertToMailMessage(fromAddress: validEmailAdress);

            // assert
            mailMessageValidation.Match(
                Succ: mailMessage => Assert.Fail("Should return failure because email to address was invalid"),
                Fail: errors => errors.Should().Contain("Email address is invalid")
            );
        }

        [Test]
        public void ShouldReturnFailureForNullInput()
        {
            // arrange
            EmailMessageDto emailMessageDto = null;

            // act
            Validation<string, MailMessage> mailMessageValidation = emailMessageDto.ConvertToMailMessage(fromAddress: validEmailAdress);

            // assert
            mailMessageValidation.Match(
                Succ: mailMessage => Assert.Fail("Should return failure because dto was null"),
                Fail: errors => errors.Should().Contain("Email message cannot be null")
            );
        }

        private void ValidateMailMessage(MailMessage mailMessage, EmailMessageDto emailMessageDto)
        {
            mailMessage.From.Should().Be((string) validEmailAdress);
            mailMessage.To.Single().Should().Be(emailMessageDto.ToAddress);
            mailMessage.Body.Should().Be(emailMessageDto.Body);
            mailMessage.Subject.Should().Be(emailMessageDto.Subject);
        }
    }
}
