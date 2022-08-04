using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Mappers.Validators;
using LanguageExt;
using System.Net.Mail;

namespace kCura.IntegrationPoints.Email.Mappers
{
    internal static class EmailMessageDtoExtensions
    {
        public static Validation<string, MailMessage> ConvertToMailMessage(
            this EmailMessageDto dto,
            ValidEmailAddress fromAddress)
        {
            if (dto == null)
            {
                return "Email message cannot be null";
            }

            Validation<string, ValidEmailAddress> emailValidation = EmailAddressValidator.ValidateEmailAddress(dto.ToAddress);

            return emailValidation.Map(validToAddress =>
                CreateMailMessage(fromAddress, validToAddress, dto.Subject, dto.Body)
            );
        }

        private static MailMessage CreateMailMessage(
            ValidEmailAddress validFromAddress,
            ValidEmailAddress validToAddress,
            string subject,
            string body)
        {
            return new MailMessage(validFromAddress, validToAddress)
            {
                Subject = subject,
                Body = body
            };
        }
    }
}
