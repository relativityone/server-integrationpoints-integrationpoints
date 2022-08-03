using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Exceptions;
using kCura.IntegrationPoints.Email.Extensions;
using kCura.IntegrationPoints.Email.Mappers;
using LanguageExt;
using Relativity.API;
using System;
using System.Net.Mail;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.Email
{
    internal class EmailSender : IEmailSender
    {
        private readonly Lazy<Either<string, ValidSmtpConfigurationDto>> _configuration;
        private readonly IAPILog _logger;
        private readonly ISmtpClientFactory _clientFactory;

        private Either<string, ValidSmtpConfigurationDto> Configuration => _configuration.Value;

        public EmailSender(
            IAPILog logger,
            ISmtpClientFactory clientFactory,
            ISmtpConfigurationProvider configurationFactory)
        {
            _logger = logger.ForContext<EmailSender>();
            _clientFactory = clientFactory;
            _configuration = new Lazy<Either<string, ValidSmtpConfigurationDto>>(
                () => ReadConfiguration(configurationFactory)
            );
        }

        public void Send(EmailMessageDto messageDto)
        {
            LogSendMethodWasInvoked();

            GetClientAndMessage(messageDto).Match(
                Right: clientAndMessage => use(
                    generator: () => clientAndMessage.client,
                    f: smtpClient => SendMessage(smtpClient, clientAndMessage.message)
                ),
                Left: LogErrorAndThrowException
            );
        }

        private Either<string, (SmtpClient client, MailMessage message)> GetClientAndMessage(EmailMessageDto messageDto)
        {
            Either<string, MailMessage> emailMessageEither = GetMailMessage(messageDto);
            Either<string, SmtpClient> clientEither = GetClient();

            return from emailMessage in emailMessageEither
                   from client in clientEither
                   select (client, emailMessage);
        }

        private Unit SendMessage(SmtpClient client, MailMessage message)
        {
            client.Send(message);
            return Unit.Default;
        }

        private Either<string, SmtpClient> GetClient()
        {
            return Configuration
                .Map(validSettings => _clientFactory.Create(validSettings.SmtpClientSettings));
        }

        private Either<string, MailMessage> GetMailMessage(EmailMessageDto messageDto)
        {
            return Configuration
                .Bind(validSettings => GetMailMessage(messageDto, validSettings.EmailFromAddres));
        }

        private Either<string, MailMessage> GetMailMessage(
            EmailMessageDto messageDto,
            ValidEmailAddress emailFromAddress)
        {
            return messageDto
                .ConvertToMailMessage(emailFromAddress)
                .ToEither(errorHeader: "SMTP email from address is invalid");
        }

        private static Either<string, ValidSmtpConfigurationDto> ReadConfiguration(ISmtpConfigurationProvider configurationProvider)
        {
            return configurationProvider
                .GetConfiguration()
                .ToEither(defaultLeftValue: "SMTP configuration is empty")
                .Bind(ConvertToValidSmtpConfiguration);
        }

        private static Either<string, ValidSmtpConfigurationDto> ConvertToValidSmtpConfiguration(SmtpConfigurationDto configuration)
        {
            return configuration
                .ConvertToSmtpClientSettings()
                .ToEither(errorHeader: "SMTP client configuration is invalid");
        }

        private void LogSendMethodWasInvoked()
        {
            _logger.LogInformation("Attempting to send an email.");
        }

        private Unit LogErrorAndThrowException(string error)
        {
            _logger.LogError("Error occured while sending email. {error}", error);
            throw new SendEmailException(error);
        }
    }
}