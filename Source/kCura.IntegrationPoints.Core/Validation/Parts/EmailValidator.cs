using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class EmailValidator : IValidator
    {
        public string Key => Constants.IntegrationPointProfiles.Validation.EMAIL;

        private const string _EMAIL_PATTERN = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[00A0D7FFF900FDCFFDF0FFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[00A0D7FFF900FDCFFDF0FFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[00A0D7FFF900FDCFFDF0FFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[00A0D7FFF900FDCFFDF0FFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[00A0D7FFF900FDCFFDF0FFEF])|(([a-z]|\d|[00A0D7FFF900FDCFFDF0FFEF])([a-z]|\d|-|\.|_|~|[00A0D7FFF900FDCFFDF0FFEF])*([a-z]|\d|[00A0D7FFF900FDCFFDF0FFEF])))\.)+(([a-z]|[00A0D7FFF900FDCFFDF0FFEF])|(([a-z]|[00A0D7FFF900FDCFFDF0FFEF])([a-z]|\d|-|\.|_|~|[00A0D7FFF900FDCFFDF0FFEF])*([a-z]|[00A0D7FFF900FDCFFDF0FFEF])))$";

        public ValidationResult Validate(object value)
        {
            var notificationEmails = value as string;

            var result = new ValidationResult();

            try
            {
                List<string> emails = (notificationEmails ?? string.Empty)
                    .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                foreach (string email in emails)
                {
                    if (string.IsNullOrEmpty(email))
                    {
                        result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_EMAIL);
                    }
                    else if (!IsValidEmail(email))
                    {
                        result.Add(IntegrationPointProviderValidationMessages.ERROR_INVALID_EMAIL + email);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, IntegrationPointProviderValidationMessages.ERROR_EMAIL_VALIDATION_EXCEPTION + ex.Message);
            }
        }

        private bool IsValidEmail(string email)
        {
            Match match = Regex.Match(email, _EMAIL_PATTERN, RegexOptions.IgnoreCase);

            return match.Success;
        }
    }
}