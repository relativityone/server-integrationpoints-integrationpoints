using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using kCura.IntegrationPoints.Email.Properties;
using Relativity.API;

namespace kCura.IntegrationPoints.Email
{
	public class SMTP : ISendable
	{
		private readonly EmailConfiguration _configuration;
		private readonly IAPILog _logger;
		private readonly ISMTPClientFactory _clientFactory;

		public SMTP(IHelper helper, ISMTPClientFactory clientFactory, EmailConfiguration configuration)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SMTP>();
			_clientFactory = clientFactory;
			_configuration = configuration;
		}

		public void Send(MailMessage message)
		{
			LogSendingEmails();
			using (SmtpClient client = GetClient())
			{
				client.Send(message);
			}
		}

		private void Validate()
		{
			var exceptions = new List<Exception>();
			if (_configuration == null)
			{
				LogMissingSmtpClientConfiguration();
				exceptions.Add(new Exception(Resources.Invalid_SMTP_Settings));
			}
			else
			{
				if (_configuration.Port < 0)
				{
					LogInvalidPortNumber(_configuration.Port);
					exceptions.Add(new Exception(Resources.SMTP_Port_Negative));
				}
				if (string.IsNullOrEmpty(_configuration.Domain))
				{
					LogInvalidDomain(_configuration.Domain);
					exceptions.Add(new Exception(Resources.SMTP_Requires_SMTP_Domain));
				}
			}
			if (exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}

		private SmtpClient GetClient()
		{
			Validate();
			var client = _clientFactory.GetClient(_configuration);
			return client;
		}

		#region Logging

		private void LogSendingEmails()
		{
			_logger.LogInformation("Attempting to send emails.");
		}

		private void LogMissingSmtpClientConfiguration()
		{
			_logger.LogError("Missing configuration for SMTP client. Skipping sending notification emails.");
		}

		private void LogInvalidDomain(string domain)
		{
			_logger.LogError("Invalid domain ({Domain}) for SMTP client. Skipping sending notification emails.", domain);
		}

		private void LogInvalidPortNumber(int port)
		{
			_logger.LogError("Invalid port number ({PortNumber}) for SMTP client. Skipping sending notification emails.", port);
		}

		#endregion
	}
}