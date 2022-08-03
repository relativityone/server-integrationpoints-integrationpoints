using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Mappers.Validators;
using kCura.IntegrationPoints.Email.Properties;
using LanguageExt;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.Email.Mappers
{
    internal static class SmtpConfigurationInstanceSettingsExtensions
    {
        private const int _MAXIMUM_PORT_NUMBER = 65535;

        public static Validation<string, ValidSmtpConfigurationDto> ConvertToSmtpClientSettings(this SmtpConfigurationDto dto)
        {
            if (dto == null)
            {
                return Resources.Invalid_SMTP_Settings;
            }

            Validation<string, int> portValidation = ConvertPort(dto.Port);
            Validation<string, string> domainValidation = ConvertDomain(dto.Domain);
            Validation<string, bool> useSslValidation = ConvertUseSsl(dto.UseSSL);
            Validation<string, ValidEmailAddress> emailFromValidation = EmailAddressValidator.ValidateEmailAddress(dto.EmailFromAddress);

            return (portValidation, domainValidation, useSslValidation, emailFromValidation)
                .Apply((validPort, validDomain, validUseSsl, validEmailFromAddress) =>
                    CreateValidSmtpConfiguration(dto, validPort, validDomain, validUseSsl, validEmailFromAddress)
                );
        }

        private static ValidSmtpConfigurationDto CreateValidSmtpConfiguration(
            SmtpConfigurationDto dto,
            int validPort,
            string validDomain,
            bool validUseSsl,
            ValidEmailAddress validEmailFromAddress)
        {
            var smtpClientSettings = new SmtpClientSettings(
                validDomain,
                validPort,
                validUseSsl,
                dto.UserName,
                dto.Password
            );

            return new ValidSmtpConfigurationDto(smtpClientSettings, validEmailFromAddress);
        }

        private static Validation<string, bool> ConvertUseSsl(bool? useSSL)
        {
            return useSSL.HasValue
                ? Success<string, bool>(useSSL.Value)
                : Fail<string, bool>(Resources.SMTP_Requires_IsSSL);
        }

        private static Validation<string, int> ConvertPort(uint? port)
        {
            if (!port.HasValue)
            {
                return Resources.SMTP_Port_Missing;
            }

            if (port > _MAXIMUM_PORT_NUMBER)
            {
                return $"Maximum allowed value for port number is: {_MAXIMUM_PORT_NUMBER}.";
            }

            return (int)port;
        }

        private static Validation<string, string> ConvertDomain(string domain)
        {
            return string.IsNullOrEmpty(domain)
                ? Fail<string, string>(Resources.SMTP_Requires_SMTP_Domain)
                : Success<string, string>(domain);
        }
    }
}
