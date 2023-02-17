using FluentAssertions;
using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Exceptions;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.IO;
using System.Net.Mail;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.Email.Tests
{
    [TestFixture, Category("Unit")]
    public class EmailSenderTests
    {
        private Mock<ISmtpConfigurationProvider> _configurationFactory;
        private Mock<ISmtpClientFactory> _clientFactory;
        private EmailSender _sut;
        private const string _INVALID_DOMAIN_NAME = "";
        private readonly EmailMessageDto _emailMessage =
            new EmailMessageDto("subject", "body", "relativity.admin@kcura.com");
        private readonly EmailMessageDto _emailMessageWithInvalidAddress =
            new EmailMessageDto("subject", "body", "this is invalid email");

        [SetUp]
        public void SetUp()
        {
            var loggerMock = new Mock<IAPILog>();
            loggerMock.Setup(x => x.ForContext<EmailSender>()).Returns(loggerMock.Object);

            _clientFactory = new Mock<ISmtpClientFactory>();
            _configurationFactory = new Mock<ISmtpConfigurationProvider>();

            _sut = new EmailSender(loggerMock.Object, _clientFactory.Object, _configurationFactory.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenConfigurationIsEmpty()
        {
            // arrange
            SetupRelativityConfigurationFactoryMock(None);

            // act
            Action act = () =>
            {
                _sut.Send(_emailMessage);
            };

            // assert
            act
                .ShouldThrow<SendEmailException>()
                .WithMessage("SMTP configuration is empty");
        }

        [Test]
        public void ShouldThrowExceptionWhenConfigurationIsInvalid()
        {
            // arrange
            var configuration = new SmtpConfigurationDto(
                domain: _INVALID_DOMAIN_NAME,
                port: 2,
                useSsl: true,
                userName: "relativity",
                password: "pass",
                emailFromAddress: "rel@rel.com"
            );
            SetupRelativityConfigurationFactoryMock(configuration);

            // act
            Action act = () =>
            {
                _sut.Send(_emailMessage);
            };

            // assert
            act
                .ShouldThrow<SendEmailException>()
                .WithMessage("SMTP client configuration is invalid. Errors: The SMTP host was not specified.");
        }

        [Test]
        public void ShouldThrowExceptionWhenEmailToAddressIsInvalid()
        {
            // arrange
            var configuration = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: 2,
                useSsl: true,
                userName: "relativity",
                password: "pass",
                emailFromAddress: "rel@rel.com"
            );
            SetupRelativityConfigurationFactoryMock(configuration);
            SetupSmtpClientFactoryToUseDefaultClient();

            // act
            Action act = () =>
            {
                _sut.Send(_emailMessageWithInvalidAddress);
            };

            // assert
            act
                .ShouldThrow<SendEmailException>()
                .WithMessage("SMTP email from address is invalid. Errors: Email address is invalid");
        }

        [Test]
        public void ShouldSendValidMessage()
        {
            // arrange
            var configuration = new SmtpConfigurationDto(
                domain: "relativity.com",
                port: 2,
                useSsl: true,
                userName: "relativity",
                password: "pass",
                emailFromAddress: "rel@rel.com"
            );
            SetupRelativityConfigurationFactoryMock(configuration);

            string outboxDirectory = GetOutboxDirectory();
            Directory.CreateDirectory(outboxDirectory);
            SetupSmtpClientFactoryToUseOutboxDirectory(outboxDirectory);

            // act
            _sut.Send(_emailMessage);

            // assert
            Directory.GetFiles(outboxDirectory).Should().ContainSingle("because mail should be sent");

            // teardown
            Directory.Delete(outboxDirectory, true);
        }

        private void SetupRelativityConfigurationFactoryMock(Option<SmtpConfigurationDto> emailConfigurationToReturn)
        {
            _configurationFactory
                .Setup(x => x.GetConfiguration())
                .Returns(emailConfigurationToReturn);
        }

        private void SetupSmtpClientFactoryToUseDefaultClient()
        {
            var smtpClient = new SmtpClient();
            _clientFactory.Setup(x => x.Create(It.IsAny<SmtpClientSettings>())).Returns(smtpClient);
        }

        private void SetupSmtpClientFactoryToUseOutboxDirectory(string outboxDirectory)
        {
            var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = outboxDirectory
            };

            _clientFactory.Setup(x => x.Create(It.IsAny<SmtpClientSettings>())).Returns(smtpClient);
        }

        private static string GetOutboxDirectory()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(dir, Guid.NewGuid().ToString());
        }
    }
}
