using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class EmailValidator : IValidator
	{
		public string Key => Constants.IntegrationPoints.Validation.EMAIL;

		public ValidationResult Validate(object value)
		{
			var notificationEmails = value as string;
			
			var invalidEmailList = new List<string>();

			//TODO Remove all white spaces
			const char tab = '\u0009';

			if (notificationEmails == null)
			{
				return new ValidationResult() { IsValid = true };
			}

			notificationEmails = notificationEmails.Replace(tab.ToString(), "");
			try
			{
				List<string> emails =
				(notificationEmails ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim())
					.ToList();

				foreach (string email in emails)
				{
					if (!IsValidEmail(email))
					{
						invalidEmailList.Add(email);
					}
				}

				if (invalidEmailList.Count > 0)
				{
					string delimiter = "; ";
					string errorMessage = "Invalid e-mails: ";
					errorMessage += string.Join(delimiter, invalidEmailList);

					return new ValidationResult()
					{
						IsValid = false,
						Message = errorMessage
					};
				}

				return new ValidationResult() { IsValid = true };
			}
			catch (Exception ex)
			{
				return new ValidationResult()
				{
					IsValid = false,
					Message = "Email Validation exception: " + ex.Message
				};
			}
		}

		private bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return true;

			//string domainMapString;
			//if (!TryMaoToDomainString(match:))


			// Use IdnMapping class to convert Unicode domain names.
			email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

			// Return true if email is in valid e-mail format.
			return Regex.IsMatch(email,
				  @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
				  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
				  RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
		}

		//private bool TryMaoToDomainString(Match match, out string value)
		//{
		//	value = string.Empty;

		//	try
		//	{
		//		// IdnMapping class with default property values.
		//		IdnMapping idn = new IdnMapping();

		//		string domainName = match.Groups[2].Value;
		//		domainName = idn.GetAscii(domainName);

		//		value= match.Groups[1].Value + domainName;

		//		return true;
		//	}
		//	catch // (ArgumentException)
		//	{

		//		return false;
		//	}
		//}

		private string DomainMapper(Match match)
		{
			// IdnMapping class with default property values.
			IdnMapping idn = new IdnMapping();

			string domainName = match.Groups[2].Value;
			domainName = idn.GetAscii(domainName);

			return match.Groups[1].Value + domainName;
		}
	}
}