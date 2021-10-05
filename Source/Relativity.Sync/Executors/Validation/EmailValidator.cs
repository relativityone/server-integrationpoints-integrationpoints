using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class EmailValidator : IValidator
	{
		private const string _EMAIL_PATTERN = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[00A0D7FFF900FDCFFDF0FFEF])+" +
									@"(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[00A0D7FFF900FDCFFDF0FFEF])+)*)|" +
									@"((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|" +
									@"\x21|[\x23-\x5b]|[\x5d-\x7e]|[00A0D7FFF900FDCFFDF0FFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|" +
									@"[00A0D7FFF900FDCFFDF0FFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|" +
									@"\d|[00A0D7FFF900FDCFFDF0FFEF])|(([a-z]|\d|[00A0D7FFF900FDCFFDF0FFEF])([a-z]|\d|-|\.|_|~|[00A0D7FFF900FDCFFDF0FFEF])" +
									@"*([a-z]|\d|[00A0D7FFF900FDCFFDF0FFEF])))\.)+(([a-z]|[00A0D7FFF900FDCFFDF0FFEF])|(([a-z]|[00A0D7FFF900FDCFFDF0FFEF])" +
									@"([a-z]|\d|-|\.|_|~|[00A0D7FFF900FDCFFDF0FFEF])*([a-z]|[00A0D7FFF900FDCFFDF0FFEF])))$";

		private const string _INVALID_EMAIL_MESSAGE = "E-mail format is invalid";
		private const string _MISSING_EMAIL_MESSAGE = "Missing email.";

		private readonly ISyncLog _logger;

		public EmailValidator(ISyncLog logger)
		{
			_logger = logger;
		}

		public Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating notification emails format.");

			ValidationResult validationResult = new ValidationResult();

			try
			{
				IEnumerable<string> emails = (configuration.GetNotificationEmails() ?? string.Empty)
					.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim());

				foreach (string email in emails)
				{
					if (string.IsNullOrEmpty(email))
					{
						_logger.LogError(_MISSING_EMAIL_MESSAGE);
						validationResult.Add(_MISSING_EMAIL_MESSAGE);
					}
					else if (!IsValidEmail(email))
					{
						_logger.LogError(_INVALID_EMAIL_MESSAGE);
						validationResult.Add(_INVALID_EMAIL_MESSAGE + ": " + email);
					}
				}
			}
			catch (Exception ex)
			{
				const string message = "Failed to validate notification emails format.";
				_logger.LogError(ex, message);
				throw;
			}

			return Task.FromResult(validationResult);
		}

		public bool ShouldValidate(ISyncPipeline pipeline) => true;

		private bool IsValidEmail(string email)
		{
			Match match = Regex.Match(email, _EMAIL_PATTERN, RegexOptions.IgnoreCase);
			return match.Success;
		}
	}
}