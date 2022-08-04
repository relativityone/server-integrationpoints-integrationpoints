using kCura.IntegrationPoints.Email.Dto;
using LanguageExt;
using System;
using System.Net.Mail;

namespace kCura.IntegrationPoints.Email.Mappers.Validators
{
    internal static class EmailAddressValidator
    {
        public static Validation<string, ValidEmailAddress> ValidateEmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return "Email address cannot be empty";
            }

            try
            {
                var mailAddress = new MailAddress(emailAddress);
                return new ValidEmailAddress(emailAddress);
            }
            catch (Exception)
            {
                return "Email address is invalid";
            }
        }
    }
}
